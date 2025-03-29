<template>
  <div class="row">
    
    <div class="col-md-12">
      <card type="dashboard-header" class="p-2">
        <div class="d-flex justify-content-between align-items-center px-2">
          <div>
            <h2 class="card-title mb-1">{{ $t("terminal.header") }}</h2>
            <p class="mb-0">{{ $t("terminal.footer") }}</p>
          </div>
        </div>
      </card>

      <card type="services">
        <div class="table-full-width table-responsive">
          <div class="terminal-container" ref="terminalContainer" @click="focusInput">
            <div class="terminal-output">
              <div v-for="(line, index) in output" :key="index" class="terminal-line">
                <span v-if="line.type === 'input'" class="terminal-input">$ {{ line.text }}</span>
                <span v-else class="terminal-response">{{ line.text }}</span>
              </div>
            </div>
            <div class="terminal-input-container">
              <span class="terminal-prompt">$</span>
              <input
                ref="inputField"
                v-model="command"
                @keyup.enter="executeCommand"
                @keyup="handleKeyUp"
                class="terminal-textbox"
                autofocus
              />
            </div>
          </div>
        </div>
      </card>
    </div>
  </div>

</template>

<script>
import apiService from "@/services/api";

export default {
  data() {
    return {
      command: "",
      output: [],
      systemInfo: null,
      commandHistory: [],
      historyIndex: -1,
    };
  },
  methods: {
    async executeCommand() {
      if (this.command.trim() === "") {
        return;
      }

      this.commandHistory.push(this.command);
      this.historyIndex = this.commandHistory.length; 

      this.output.push({ type: "input", text: this.command });
      const commandLower = this.command.trim().toLowerCase();

      if (commandLower === "clear") {
        this.output = [];
        this.command = "";
        this.scrollToBottom();
        return;
      }

      try {
        const response = await apiService.execCommand(this.command, this.systemInfo);
        if (response && response.output) {
          this.output.push({ type: "response", text: response.output });
        }
        this.command = "";
      } catch (error) {
        console.error("Command execution error:", error);
        this.output.push({ type: "response", text: `Error: ${error.message || error}` });
        this.command = "";
      }

      this.scrollToBottom();
    },
    handleKeyUp(event) {
      if (event.ctrlKey && event.key === "c") {
        this.output.push({ type: "response", text: "$ " + this.command + "^C" });
        this.command = ""; 
      } else if (event.key === "ArrowUp") {
        if (this.historyIndex > 0) {
          this.historyIndex--;
          this.command = this.commandHistory[this.historyIndex];
        }
      } else if (event.key === "ArrowDown") {
        if (this.historyIndex < this.commandHistory.length - 1) {
          this.historyIndex++;
          this.command = this.commandHistory[this.historyIndex];
        } else {
          this.historyIndex = this.commandHistory.length;
          this.command = "";
        }
      }
    },
    focusInput() {
      this.$refs.inputField.focus();
    },
    scrollToBottom() {
      this.$nextTick(() => {
        this.$refs.terminalContainer.scrollTop = this.$refs.terminalContainer.scrollHeight;
      });
    },
  },
  mounted() {
    this.systemInfo = this.$store.getters.currentSystem;
    if (!this.systemInfo) {
      this.output.push({ type: "response", text: "System information not available." });
    }
    this.scrollToBottom();
  },
  updated() {
    this.scrollToBottom();
  },
};
</script>

<style scoped>
.terminal-container {
  background-color: black;
  color: rgb(255, 255, 255);
  font-family: monospace;
  padding: 10px;
  width: 100%;
  height: 700px;
  overflow-y: auto;
  border-radius: 5px;
}
.terminal-output {
  white-space: pre-wrap;
  overflow-wrap: break-word;
}
.terminal-line {
  margin: 2px 0;
}
.terminal-input-container {
  display: flex;
  align-items: center;
}
.terminal-prompt {
  color: rgb(255, 255, 255);
  margin-right: 5px;
}
.terminal-textbox {
  background: transparent;
  border: none;
  color: rgb(255, 255, 255);
  font-family: monospace;
  outline: none;
  width: 100%;
}
</style>