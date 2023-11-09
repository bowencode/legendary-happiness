<script lang="ts">
import { UserManager } from "oidc-client-ts";

export default {
  name: "app",
  data() {
    return {
      loggedIn: false,
      userManager: new UserManager({
        authority: `https://localhost:5001`,
        client_id: "spa",
        redirect_uri: `https://localhost:5173/signin-callback`,
        scope: `openid profile scope1`,
      }),
      accessToken: '',
      idToken: ''
    };
  },
  methods: {
    logout() {
      this.userManager.signoutRedirect();
    },
    login() {
      this.userManager.signinRedirect();
    },
  },
  beforeMount() {
    if (window.location.pathname === "/signin-callback") {
      this.userManager.signinRedirectCallback().then((user) => {
        if (!!user && !user.expired) {
          this.loggedIn = true;
          console.log(user);
          window.location.href = window.location.origin;
          this.accessToken = user.access_token;
          this.idToken = user.id_token?? '';
        }
      });
    } else {
      const user = this.userManager.getUser().then(user => {
        if (!!user && !user.expired) {
          this.loggedIn = true;
          this.accessToken = user.access_token;
          this.idToken = user.id_token;
        }
      });
    }
  }
};
</script>

<template>
  <header>
    <div class="header-container">
      <div class="header-left-container">
        <div class="app-title">Sample Vue Client</div>
      </div>
      <div class="header-right-container">
        <button v-if="loggedIn" @click="logout()" class="loginout-button">
          Logout
        </button>
        <button v-if="!loggedIn" @click="login()" class="loginout-button">
          Login
        </button>
      </div>
    </div>
  </header>
  <main>
    <h2 v-if="!!accessToken">Access Token</h2>
    <div>{{ accessToken }}</div>
    <h2 v-if="!!idToken">ID Token</h2>
    <div>{{ idToken }}</div>
  </main>
</template>

<style scoped>
.header-container {
  display: flex;
  justify-content: space-between;
  background-color: cornflowerblue;
  font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
}

.header-left-container {
  display: flex;
  align-items: center;
}

.header-right-container {
  display: flex;
  flex-direction: row-reverse;
  align-items: center;
  margin-right: 50px;
}

.app-title {
  font-size: 2em;
  font-weight: bold;
  margin: 10px;
}

.header-link {
  font-size: 1em;
  font-weight: normal;
  margin: 10px;
}

.loginout-button {
  font-size: 1em;
  font-weight: normal;
  margin: 10px;
  width: 75px;
}
</style>
