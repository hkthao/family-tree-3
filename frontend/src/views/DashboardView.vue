<template>
  <v-container fluid>
    <v-row>
      <!-- Greeting Card -->
      <v-col cols="12">
        <v-card class="pa-4 rounded-xl" elevation="2">
          <v-row align="center">
            <v-col cols="12" md="8">
              <h3 class="text-h5 font-weight-bold">Congratulations John! 🎉</h3>
              <p class="text-subtitle-1 text-grey-darken-1 mt-2">You have done 72% more sales today.<br>Check your new badge in your profile.</p>
              <v-btn color="primary" class="mt-4">View Badges</v-btn>
            </v-col>
            <v-col cols="12" md="4" class="text-center">
              <v-img src="https://demos.themeselection.com/sneat-vuetify-vuejs-admin-template/demo-1/assets/illustration-john-light-0061869a.png" max-height="150"></v-img>
            </v-col>
          </v-row>
        </v-card>
      </v-col>

      <!-- Statistics Cards -->
      <v-col cols="12" md="8">
        <v-row>
          <v-col cols="12" md="4">
            <StatisticCard title="Profit" value="$12,628" change="+72.8%" icon="mdi-poll" color="primary" />
          </v-col>
          <v-col cols="12" md="4">
            <StatisticCard title="Sales" value="$4,679" change="+28.2%" icon="mdi-currency-usd" color="success" />
          </v-col>
          <v-col cols="12" md="4">
            <StatisticCard title="Payments" value="$2,465" change="-14.8%" icon="mdi-credit-card-outline" color="error" />
          </v-col>
        </v-row>
      </v-col>

      <v-col cols="12" md="4">
        <ChartCard title="Growth">
          <v-progress-circular :model-value="78" :size="150" :width="15" color="primary" class="mx-auto">
            <span class="text-h5">78%</span>
          </v-progress-circular>
        </ChartCard>
      </v-col>

      <!-- Total Revenue -->
      <v-col cols="12" md="8">
        <ChartCard title="Total Revenue">
          <p class="text-center">Bar chart placeholder</p>
          <v-progress-linear model-value="65" class="my-2" color="primary" height="10"></v-progress-linear>
          <v-progress-linear model-value="45" class="my-2" color="secondary" height="10"></v-progress-linear>
        </ChartCard>
      </v-col>

      <!-- Profile Report -->
      <v-col cols="12" md="4">
        <ChartCard title="Profile Report">
          <p class="text-center">Line chart placeholder</p>
          <v-progress-linear model-value="80" class="my-2" color="warning" height="10"></v-progress-linear>
        </ChartCard>
      </v-col>

      <!-- Order Statistics -->
      <v-col cols="12" md="6">
        <ChartCard title="Order Statistics">
          <p class="text-center">Pie chart placeholder</p>
          <v-progress-circular :model-value="60" :size="100" :width="10" color="success" class="mx-auto"></v-progress-circular>
        </ChartCard>
      </v-col>

      <!-- Transactions -->
      <v-col cols="12" md="6">
        <ChartCard title="Transactions">
          <p class="text-center">List placeholder</p>
          <v-list-item title="Paypal" subtitle="Sent"><template v-slot:append><span class="text-success">+$24.82</span></template></v-list-item>
          <v-list-item title="Credit Card" subtitle="Received"><template v-slot:append><span class="text-error">-$12.45</span></template></v-list-item>
          <v-btn block class="mt-4 bg-gradient-primary" dark>Upgrade to Pro</v-btn>
        </ChartCard>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import StatisticCard from '@/components/dashboard/StatisticCard.vue';
import ChartCard from '@/components/dashboard/ChartCard.vue';
</script>

<style>
.bg-gradient-primary {
  background: linear-gradient(45deg, #696CFF, #8592A3);
  color: white;
}
</style>