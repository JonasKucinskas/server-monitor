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
            :path="currentPath"
            :fileSystemData="fileData"
            :view="view"
            :toolbarSettings="toolbarSettings"
            :contextMenuSettings="contextMenuSettings"
            @delete="onDelete"
            @rename="onRename"
            @folderCreate="onFolderCreate"
            @refreshFiles="onFilesRefresh"
            @beforeDownload="onBeforeDownload"
            @beforeMove="onBeforeMove"
          ></ejs-filemanager>
        </div>
      </card>
    </div>
  </div>
</template>

<script>
import { FileManagerComponent, NavigationPane, Toolbar, DetailsView } from "@syncfusion/ej2-vue-filemanager";
import apiService from "@/services/api";

export default {
  components: {
    "ejs-filemanager": FileManagerComponent,
  },
  data() {
    return {
        currentPath: "",
        fileData: [],
        currentSystem: [],
        toolbarSettings: {
            items: ["NewFolder", "SortBy", "Cut", "Copy", "Paste", "Delete", "Refresh", "Download", "Rename", "Selection", "View", "Details"],
        },
        contextMenuSettings: {
            file: ["Cut", "Copy", "|", "Delete", "Download", "Rename", "|", "Details"],
            layout: ["SortBy", "View", "Refresh", "|", "Paste", "|", "NewFolder", "|", "Details", "|", "SelectAll"],
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
        };
    },
    provide: {
        filemanager: [NavigationPane, DetailsView, Toolbar],
    },
    async mounted() {
        this.currentSystem = this.$store.getters.currentSystem;
        
        await this.loadFileStructure();
    },
    methods: {
        async onBeforeDownload(args){
            args.cancel = true;
            
            const file = args.data.names[0];
            if (!file) return;

            try {//single file download only
                const response = await apiService.execCommand(`base64 ${args.data.names[0]}`, this.currentSystem);

                const binaryData = atob(response.output);

                const byteArray = new Uint8Array(binaryData.length);
                for (let i = 0; i < binaryData.length; i++) {
                    byteArray[i] = binaryData.charCodeAt(i);
                }

                const blob = new Blob([byteArray], {});
                const url = URL.createObjectURL(blob);

                const link = document.createElement('a');
                link.href = url;
                link.download = args.data.names[0];
                link.click(); 

                URL.revokeObjectURL(url);
            } catch (error) {
                console.error("Download failed:", error);
            }

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
            console.logs("refresh");
            await this.loadFileStructure();
        },
        async onBeforeMove(args) {
            if (args.isCopy === true){
                if (args.itemData[0].isFile){
                    await apiService.execCommand(`cp ${args.itemData[0].name} ${args.targetData.name}`, this.currentSystem);
                }
                else await apiService.execCommand(`cp -r ${args.itemData[0].name} ${args.targetData.name}`, this.currentSystem);
            }
            else{
                await apiService.execCommand(`mv ${args.itemData[0].name} ${args.targetData.name}`, this.currentSystem);
            }
        },
        async loadFileStructure() {
            try {
                this.fileData = [];
                const pwd = await apiService.execCommand('pwd', this.currentSystem);
                const lastPart = pwd.output.substring(pwd.output.lastIndexOf('/') + 1);

                const parent = await apiService.execCommand(`ls -l --time-style=+%Y-%m-%dT%H:%M:%S ../ | grep ${lastPart}}`, this.currentSystem);
                
                this.currentPath = "parent.output";
                this.parseLsOutput(parent.output, null);
                const parentId = this.fileData[0].id;
                
                //add children
                const response = await apiService.execCommand(`ls -l --time-style=+%Y-%m-%dT%H:%M:%S ${pwd.output}}`, this.currentSystem);
                this.parseLsOutput(response.output, parentId);
            } catch (error) {
                console.error("Error loading file structure:", error);
            }
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
                        read: permissions.charAt(1) === 'r',
                        write: permissions.charAt(2) === 'w',
                        delete: permissions.charAt(3) === 'x',
                        download: true,
                        copy: true,
                        read: true,
                        upload: true,
                        writeContents: true
                    },
                    size,
                    type: isFile ? '.' + name.split('.').pop() : '', 
                };
                
                this.fileData.push(fileObj); 

            });
            this.fileData = [...this.fileData];
        }
    },
};
</script>