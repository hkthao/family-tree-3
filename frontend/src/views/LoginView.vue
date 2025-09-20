<template>
  <v-app>
    <v-main class="d-flex align-center justify-center">
      <v-container fluid>
        <v-row justify="center">
          <v-col cols="12" sm="8" md="6" lg="4">
            <v-card class="elevation-12">
              <v-card-title class="text-center text-h5 py-4">
                <span class="font-weight-bold">FamilyTree</span>
              </v-card-title>
              <v-card-subtitle class="text-center">
                Welcome to FamilyTree! 👋
              </v-card-subtitle>
              <v-card-text class="px-6 py-8">
                <LoginForm @login-success="onLoginSuccess" @login-fail="onLoginFail" />

                <v-divider class="my-4">or</v-divider>

                <SocialLogin />
              </v-card-text>
              <v-card-actions class="justify-center pb-4">
                New on our platform?
                <router-link to="/register" class="text-primary ml-1">Create an account</router-link>
              </v-card-actions>
            </v-card>
          </v-col>
        </v-row>
      </v-container>
    </v-main>
    <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
      {{ snackbar.message }}
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import LoginForm from '@/components/auth/LoginForm.vue';
import SocialLogin from '@/components/auth/SocialLogin.vue';
</script>