<template>
  <div class="row">
    <div class="col-md-12">
      <card type="dashboard-header" class="p-2">
        <div class="d-flex justify-content-between align-items-center px-2">
          <div class="col-10">
            <h2 class="card-title mb-1">{{ $t("services.header") }}</h2>
            <p class="mb-0">{{ $t("services.footer") }}</p>
          </div>
        </div>
      </card>
    </div>

    <div class="col-md-12">
      <card type="services">
        <template slot="header"></template>

        <div id="app">
          <ejs-filemanager
            ref="fileManager"
            id="overview_file"
            :path="initialPath"
            :fileSystemData="fileData"
            :view="view"
            :toolbarSettings="toolbarSettings"
            :contextMenuSettings="contextMenuSettings"
            :uploadSettings="uploadSettings"
            :navigationPaneSettings="navigationPaneSettings"
            @fileOpen="onDirOpened"
            @delete="onDelete"
            @rename="onRename"
            @folderCreate="onFolderCreate"
            @refreshFiles="onFilesRefresh"
            @beforeDownload="onBeforeDownload"
            @beforeMove="onBeforeMove"
            @uploadListCreate="onBeforeSendRequest"
          ></ejs-filemanager>
        </div>
      </card>
    </div>
  </div>
</template>

<script>
import { FileManagerComponent, NavigationPane, Toolbar, DetailsView, NavigationPaneSettings, UploadSettings } from "@syncfusion/ej2-vue-filemanager";
import apiService from "@/services/api";

export default {
  components: {
    "ejs-filemanager": FileManagerComponent,
  },
  data() {
    return {
        initialPath: "/home",
        currentPath: "/home",
        fileData: [],
        alreadyLoadedFiles: [],
        currentSystem: [],
        toolbarSettings: {
            items: ["NewFolder", "Upload", "Cut", "Copy", "Paste", "Delete", "Refresh", "Download", "Rename", "Selection", "Details"],
        },
        contextMenuSettings: {
            file: ["Cut", "Copy", "|", "Delete", "Download", "Rename", "|", "Details"],
            layout: ["SortBy", "View", "Refresh", "|", "Paste", "|", "NewFolder", "|", "Uplpad", "|", "Details", "|", "SelectAll"],
            visible: true,
        },
        view: "Details",
        detailsViewSettings: {
            columns: [
            {
                field: "name",
                headerText: "Name",
                customAttributes: { class: "e-fe-grid-name" },
            },
            {
                field: "dateModified",
                headerText: "Date Modified",
                format: "MM/dd/yyyy hh:mm a",
            },
            {
                field: "size",
                headerText: "Size",
                template: '<span class="e-fe-size">${size}</span>',
                format: "n2",
            },
            ],
        },
        uploadSettings:{
          autoUpload: true
        },
        navigationPaneSettings:{
          visible: false
        }
      };
    },
    provide: {
        filemanager: [NavigationPane, DetailsView, Toolbar],
    },
    async mounted() {
        this.currentSystem = this.$store.getters.currentSystem;
        
        await this.loadFileStructure(this.currentPath);
    },
    methods: {
        async onDirOpened(args){
          console.log(args);

          if (args.fileDetails.isFile){
            return;
          }

          const newPath = args.fileDetails.name;

          let pathParts = this.currentPath.split('/').filter(Boolean);
          let containsSubstring = pathParts.some(part => part === newPath);

          console.log(pathParts);

          if (!containsSubstring)
          {
            console.log("does not contains substricng");
            this.currentPath += "/"+newPath;
            if (!this.alreadyLoadedFiles.includes(this.currentPath))
            {
              console.log("not includes path");
              this.alreadyLoadedFiles.push(this.currentPath);
              await this.loadSubDir(this.currentPath, args.fileDetails.id);
            }
          }
          else {
            console.log("contains substricng");
            let newPathIndex = this.currentPath.indexOf(newPath);
            this.currentPath = this.currentPath.substring(0, newPathIndex + newPath.length);
          }


          console.log(this.currentPath);
        },
        async onBeforeSendRequest(args){
          const rawFile = args.fileInfo.rawFile;
          
          if (rawFile instanceof File) {
            const base64File = await this.convertFileToBase64(rawFile);

            const chunkSize = 32768 - new TextEncoder().encode(`printf "%s" "" | base64 -d >> ${args.fileInfo.name}`).length;
            const totalChunks = Math.ceil(new TextEncoder().encode(base64File).length / chunkSize);
            
            for (let i = 0; i < totalChunks; i++) 
            {
              const chunk = base64File.slice(i * chunkSize, (i + 1) * chunkSize);
              await apiService.execCommand(`printf "%s" "${chunk}" | base64 -d >> ${args.fileInfo.name}`, this.currentSystem);
            }
          }
          else{
            console.log("not File format");
          }
        },
        async convertFileToBase64(file) {
          return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
              const base64String = reader.result.split(',')[1];
              resolve(base64String); 
            };
            reader.onerror = reject; 
            reader.readAsDataURL(file);  
          });
        },
        async onBeforeDownload(args){
            args.cancel = true;
            
            const fileName = args.data.names[0];
            if (!fileName) return;

            const response = await apiService.execCommand(`base64 ${this.currentPath}/${fileName}`, this.currentSystem);

            const byteArray = Uint8Array.from(atob(response.output), c => c.charCodeAt(0));
            const blob = new Blob([byteArray]);

            const a = document.createElement("a");
            a.href = URL.createObjectURL(blob);
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);

        },
        async onDelete(args) {
            if (args.itemData[0].isFile === false)
            {
                await apiService.execCommand(`rm -rf ${args.itemData[0].name}`, this.currentSystem);
            }
            else await apiService.execCommand(`rm ${args.itemData[0].name}`, this.currentSystem);
        },
        async onRename(args) {
            await apiService.execCommand(`mv ${args.itemData[0].id} ${args.newName}`, this.currentSystem);
        },
        async onFolderCreate(args) {
            await apiService.execCommand(`mkdir ${args.folderName}`, this.currentSystem);
        },
        async onFilesRefresh(args) {
            //console.logs("refresh");
            //await this.loadFileStructure();
        },
        async onBeforeMove(args) {
            if (args.isCopy === true)
            {
                if (args.itemData[0].isFile)
                {
                    await apiService.execCommand(`cp ${args.itemData[0].name} ${args.targetData.name}`, this.currentSystem);
                }
                else await apiService.execCommand(`cp -r ${args.itemData[0].name} ${args.targetData.name}`, this.currentSystem);
            }
            else
            {
                await apiService.execCommand(`mv ${args.itemData[0].name} ${args.targetData.name}`, this.currentSystem);
            }
        },
        async loadSubDir(path, parentId){
          const parts = this.splitPath(path);
          
          
          const response = await apiService.execCommand(`ls -l --time-style=+%Y-%m-%dT%H:%M:%S ${parts.parent}/${parts.name}`, this.currentSystem);
          this.parseLsOutput(response.output, parentId);
        },
        async loadFileStructure(path) {
            this.alreadyLoadedFiles.push(path);
            try {
                this.fileData = [];

                const parts = this.splitPath(path);
                const response = await apiService.execCommand(`ls -l --time-style=+%Y-%m-%dT%H:%M:%S ${parts.parent} | grep ${parts.name}`, this.currentSystem);
                const parent = response.output;

                console.log(parts);

                this.parseLsOutput(parent, null);
                const parentId = this.fileData[0].id;
                
                //add children
                const response2 = await apiService.execCommand(`ls -l --time-style=+%Y-%m-%dT%H:%M:%S ${parts.parent}/${parts.name}`, this.currentSystem);
                this.parseLsOutput(response2.output, parentId);
            } catch (error) {
                console.error("Error loading file structure:", error);
            }
        },
        splitPath(path) {
          if (path.length > 1) {
            path = path.replace(/\/+$/, '');
          }

          const parts = path.split('/');
          let parent, name;

          if (parts.length <= 2) {
            parent = '/';
            name = parts[1] || '';
          } else {
            name = parts.pop();
            parent = parts.join('/') || '/';
          }

          return { parent, name };
        },
        parseLsOutput(lsOutput, parentId) {
            const lines = lsOutput.trim().split('\n');

            const dataLines = lines.length > 1 ? lines.slice(1) : lines;

            dataLines.forEach(line => {
                const parts = line.split(/\s+/);

                const permissions = parts[0];
                const isFile = permissions.charAt(0) === '-'; 
                const name = parts[6]; 
                const size = parseInt(parts[4], 10); 
                const dateModified = new Date(parts[5]); 
                const isFolder = !isFile;

                const fileObj = {
                    dateCreated: dateModified, 
                    dateModified,
                    filterPath: '/' + name, 
                    hasChild: isFolder,
                    id: name, 
                    imageUrl: '',
                    isFile,
                    name,
                    parentId: parentId,
                    permission: {
                        read: true,
                        write: true,
                        delete: true,
                        download: true,
                        copy: true,
                        upload: true,
                        writeContents: true
                    },
                    size,
                    type: isFile ? '.' + name.split('.').pop() : '', 
                };
                
                if (fileObj.name != null){
                  this.fileData.push(fileObj); 
                }

            });
            console.log(this.fileData);
            this.fileData = [...this.fileData];
        }
    },
};
</script>

<style>
#overview_file {
  min-height: 660px;
}
.e-filemanager{
    background: #344675,
}
.e-filemanager .e-toolbar {
  border-bottom-color: #344675;
}
.e-toolbar {
  -webkit-tap-highlight-color: #1d1d2c;
  background: #1d1d2c;
}
.e-filemanager .e-address .e-address-list-item:last-child .e-list-text {
  color: #ffffff;
}
.e-filemanager .e-address {
  background: #1d1d2c;
}
.e-navigation {
  background: #1d1d2c;
}
.e-grid .e-content {
  background-color: #1d1d2c;
}
.e-grid .e-table {
  background-color: #1d1d2c;
}
.e-fe-text {
    color:#ffffff
}
.e-row:hover .e-fe-text {
  color: #000000;
}

.e-rowcell {
  color: #ffffff !important;
}

.e-row:hover .e-rowcell {
  color: #000000 !important;
}
</style>