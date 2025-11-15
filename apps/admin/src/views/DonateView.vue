<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="text-h5">
            {{ $t('donate.title') }}
          </v-card-title>
          <v-card-text>
            <p>{{ $t('donate.description') }}</p>

            <v-row>
              <v-col cols="12" md="6">
                <v-card flat>
                  <v-card-title class="text-h6">{{ $t('donate.buyMeACoffee') }}</v-card-title>
                  <v-card-text class="text-center">
                    <v-img
                      src="/bmc_qr.png"
                      alt="Buy Me A Coffee QR Code"
                      max-width="200"
                      height="200"
                      cover
                      class="mx-auto mb-4"
                      rounded="lg"
                    ></v-img>
                    <a href="https://www.buymeacoffee.com/thaohk90e" target="_blank">
                      https://www.buymeacoffee.com/thaohk90e
                    </a>
                  </v-card-text>
                </v-card>
              </v-col>

              <v-col cols="12" md="6">
                <v-card flat>
                  <v-card-title class="text-h6">{{ $t('donate.momo') }}</v-card-title>
                  <v-card-text class="text-center">
                    <!-- Placeholder for MoMo QR code -->
                    <v-img
                      src="/momo.jpg"
                      alt="MoMo QR Code"
                      width="200"
                      height="200"
                      class="mx-auto mb-4"
                      rounded="lg"
                      cover
                    ></v-img>
                    <p>0946351139</p>

                  </v-card-text>
                </v-card>
              </v-col>
            </v-row>

            <p class="mt-4">{{ $t('donate.thankYou') }}</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
// No script logic needed for a static donate page
</script>

<style scoped>
/* Add any specific styles for the donate page here */
</style>
