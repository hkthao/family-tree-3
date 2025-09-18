<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card color="primary" dark class="mb-4" elevation="2">
          <v-card-title class="text-h5">Congratulations John! 🎉</v-card-title>
          <v-card-text>
            You have done 72% more sales today. Check your new badge in your profile.
          </v-card-text>
          <v-card-actions>
            <v-btn text>View Badges</v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="3" sm="6">
        <StatisticCard
          title="Profit"
          value="$12,628"
          icon="mdi-currency-usd"
          color="success"
        />
      </v-col>
      <v-col cols="12" md="3" sm="6">
        <StatisticCard
          title="Sales"
          value="$4,679"
          icon="mdi-chart-line"
          color="info"
        />
      </v-col>
      <v-col cols="12" md="3" sm="6">
        <StatisticCard
          title="Payments"
          value="$2,367"
          icon="mdi-credit-card-outline"
          color="warning"
        />
      </v-col>
      <v-col cols="12" md="3" sm="6">
        <StatisticCard
          title="Transactions"
          value="1,485"
          icon="mdi-swap-horizontal"
          color="error"
        />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="8">
        <ChartCard
          title="Total Revenue"
          subtitle="Yearly Growth"
          type="bar"
          :chart-data="[
            { label: '2023', value: 70 },
            { label: '2024', value: 90 },
          ]"
        />
      </v-col>
      <v-col cols="12" md="4">
        <ChartCard
          title="Growth"
          subtitle="Overall Progress"
          type="progress"
          :chart-data="[
            { label: 'Growth', value: 78 },
          ]"
        />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <ChartCard
          title="Profile Report"
          subtitle="Monthly Performance"
          type="line"
          :chart-data="[
            { label: 'Jan', value: 60 },
            { label: 'Feb', value: 75 },
            { label: 'Mar', value: 50 },
            { label: 'Apr', value: 80 },
            { label: 'May', value: 65 },
          ]"
        />
      </v-col>
      <v-col cols="12" md="6">
        <v-card elevation="2">
          <v-card-title>Order Statistics</v-card-title>
          <v-card-text>
            <v-list>
              <v-list-item>
                <v-list-item-title>Total Orders</v-list-item-title>
                <v-list-item-subtitle>12,345</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Pending Orders</v-list-item-title>
                <v-list-item-subtitle>567</v-list-item-subtitle>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>Completed Orders</v-list-item-title>
                <v-list-item-subtitle>11,778</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title>Transactions</v-card-title>
          <v-card-text>
            <v-data-table
              :headers="transactionHeaders"
              :items="transactions"
              item-value="id"
            ></v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import StatisticCard from '@/components/layout/StatisticCard.vue';
import ChartCard from '@/components/layout/ChartCard.vue';

const transactionHeaders = [
  { title: 'Transaction ID', key: 'id' },
  { title: 'Amount', key: 'amount' },
  { title: 'Status', key: 'status' },
  { title: 'Date', key: 'date' },
];

const transactions = [
  { id: '#12345', amount: '$100', status: 'Completed', date: '2023-01-15' },
  { id: '#12346', amount: '$50', status: 'Pending', date: '2023-01-16' },
  { id: '#12347', amount: '$200', status: 'Completed', date: '2023-01-17' },
  { id: '#12348', amount: '$75', status: 'Failed', date: '2023-01-18' },
];
</script>