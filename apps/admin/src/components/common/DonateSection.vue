<template>
  <v-row>
    <v-col cols="12">
      <v-card class="h-100" flat>
        <v-card-title class="text-h6 text-secondary">{{ $t('about.donate.title') }}</v-card-title>
        <v-card-text>
          <p class="text-body-1 mb-4">
            {{ $t('about.donate.description') }}
          </p>
          <p class="text-body-1 mb-4">
            {{ $t('about.donate.reason') }}
          </p>
          <v-row>
            <v-col cols="12" md="6">
              <v-card flat>
                <v-card-title class="text-h6 text-center">{{ $t('donate.buyMeACoffee') }}</v-card-title>
                <v-card-text class="text-center">
                  <v-img src="/bmc_qr.png" alt="Buy Me A Coffee QR Code" max-width="200" height="200"
                    class="mx-auto mb-4" rounded="lg"></v-img>
                  <a href="https://www.buymeacoffee.com/thaohk90e" target="_blank">
                    https://www.buymeacoffee.com/thaohk90e
                  </a>
                </v-card-text>
              </v-card>
            </v-col>

            <v-col cols="12" md="6">
              <v-card flat>
                <v-card-title class="text-h6 text-center">{{ $t('donate.momo') }}</v-card-title>
                <v-card-text class="text-center">
                  <v-img src="/momo.jpg" alt="MoMo QR Code" max-width="200" height="200" class="mx-auto mb-4"
                    rounded="lg" cover></v-img>
                  <p>0946351139</p>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
          <p class="text-body-1 mt-4">
            {{ $t('about.donate.thankYou') }}
          </p>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
// No script logic needed for this component
</script>

<style scoped>
/* Add any specific styles for the donate section here */
</style>
