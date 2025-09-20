<template>
  <v-app>
    <v-main>
      <v-container fluid fill-height class="d-flex justify-center align-center">
        <v-row class="text-center" justify="center">
          <v-col cols="12">
            <h1 class="text-h1 font-weight-bold">404</h1>
            <div class="text-h4">{{ t('notFound.title') }}</div>
            <p class="mt-3">
              {{ t('notFound.subtitle') }}
            </p>
          </v-col>

          <v-col cols="12" class="my-5">
            <v-img
              :src="image404"
              max-width="600"
              class="mx-auto"
            ></v-img>
          </v-col>

          <v-col cols="12">
            <v-btn
              color="blue-darken-2"
              size="large"
              class="text-none"
              rounded="lg"
              to="/"
            >
              {{ t('notFound.backToHome') }}
            </v-btn>
          </v-col>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
</template>

<script lang="ts" setup>
import image404 from '@/assets/images/404.png';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>

<style scoped>
.text-h1 {
  font-size: 8rem !important;
}
</style>
