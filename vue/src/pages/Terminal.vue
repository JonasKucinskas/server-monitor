<template>
    <card type="services">
        <div class="table-full-width table-responsive">
            <div class="terminal-container">
                <div class="terminal-output">
                <div v-for="(line, index) in output" :key="index" class="terminal-line">
                    <span v-if="line.type === 'input'" class="terminal-input">$ {{ line.text }}</span>
                    <span v-else class="terminal-response">{{ line.text }}</span>
                </div>
                </div>
                <div class="terminal-input-container">
                <span class="terminal-prompt">$</span>
                <input
                    v-model="command"
                    @keyup.enter="executeCommand"
                    class="terminal-textbox"
                    autofocus
                />
                </div>
            </div>
        </div>
    </card>
</template>

<script>
import apiService from "@/services/api";

export default {
  data() {
    return {
      command: "",
      output: []
    };
  },
  methods: {
     
    async executeCommand() {
      if (this.command.trim() === "") return;
      
      // Add user input to output
      this.output.push({ type: "input", text: this.command });
      try {
        const response = await apiService.execCommand(this.command, this.systemInfo);
        console.log(response);
        this.output.push({ type: "response", text: response.output });
        this.command = "";

      } catch (error) {
        console.log(error);
        this.command = "";
      }

    }
  },
  mounted(){
    this.systemInfo = this.$store.getters.currentSystem;
  } 
};
</script>

<style scoped>
.terminal-container {
  background-color: black;
  color: rgb(255, 255, 255);
  font-family: monospace;
  padding: 10px;
  width: 100%;
  height: 860px;
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
  