<template>
  <v-container fluid>
    <v-row>
      <!-- Greeting Card -->
      <v-col cols="12">
        <v-card class="pa-4 rounded-xl" elevation="2">
          <v-row align="center">
            <v-col cols="12" md="8">
              <h3 class="text-h5 font-weight-bold">{{ $t('dashboard.greeting', { name: 'John' }) }} 🎉</h3>
              <p class="text-subtitle-1 text-grey-darken-1 mt-2">{{ $t('dashboard.greetingSubtitle') }}</p>
              <v-btn color="primary" class="mt-4">{{ $t('dashboard.viewBadges') }}</v-btn>
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
            <StatisticCard :title="$t('dashboard.profit')" value="$12,628" change="+72.8%" icon="mdi-poll" color="primary" />
          </v-col>
          <v-col cols="12" md="4">
            <StatisticCard :title="$t('dashboard.sales')" value="$4,679" change="+28.2%" icon="mdi-currency-usd" color="success" />
          </v-col>
          <v-col cols="12" md="4">
            <StatisticCard :title="$t('dashboard.payments')" value="$2,465" change="-14.8%" icon="mdi-credit-card-outline" color="error" />
          </v-col>
        </v-row>
      </v-col>

      <v-col cols="12" md="4">
        <ChartCard :title="$t('dashboard.growth')">
          <VueApexCharts type="radialBar" :options="growthChartOptions" :series="growthChartSeries"></VueApexCharts>
        </ChartCard>
      </v-col>

      <!-- Total Revenue -->
      <v-col cols="12" md="8">
        <ChartCard :title="$t('dashboard.totalRevenue')">
          <VueApexCharts type="bar" :options="revenueChartOptions" :series="revenueChartSeries"></VueApexCharts>
        </ChartCard>
      </v-col>

      <!-- Profile Report -->
      <v-col cols="12" md="4">
        <ChartCard :title="$t('dashboard.profileReport')">
          <VueApexCharts type="line" :options="profileChartOptions" :series="profileChartSeries"></VueApexCharts>
        </ChartCard>
      </v-col>

      <!-- Order Statistics -->
      <v-col cols="12" md="6">
        <ChartCard :title="$t('dashboard.orderStatistics')">
          <VueApexCharts type="pie" :options="orderChartOptions" :series="orderChartSeries"></VueApexCharts>
        </ChartCard>
      </v-col>

      <!-- Transactions -->
      <v-col cols="12" md="6">
        <ChartCard :title="$t('dashboard.transactions')">
          <p class="text-center">List placeholder</p>
          <v-list-item title="Paypal" subtitle="Sent"><template v-slot:append><span class="text-success">+$24.82</span></template></v-list-item>
          <v-list-item title="Credit Card" subtitle="Received"><template v-slot:append><span class="text-error">-$12.45</span></template></v-list-item>
          <v-btn block class="mt-4 bg-gradient-primary" dark>{{ $t('dashboard.upgradeToPro') }}</v-btn>
        </ChartCard>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { StatisticCard, ChartCard } from '@/components/dashboard';
import VueApexCharts from 'vue3-apexcharts';

const { t } = useI18n();

// Growth Chart (RadialBar)
const growthChartOptions = ref({
  chart: {
    height: 280,
    type: 'radialBar',
  },
  plotOptions: {
    radialBar: {
      hollow: {
        size: '70%',
      },
      dataLabels: {
        show: true,
        name: {
          show: false,
        },
        value: {
          show: true,
          fontSize: '24px',
          fontWeight: 'bold',
          offsetY: 8,
        },
      },
    },
  },
  fill: {
    type: 'gradient',
    gradient: {
      shade: 'dark',
      type: 'horizontal',
      shadeIntensity: 0.5,
      gradientToColors: ['#8592A3'],
      inverseColors: true,
      opacityFrom: 1,
      opacityTo: 1,
      stops: [0, 100],
    },
  },
  stroke: {
    lineCap: 'round',
  },
  labels: ['Growth'],
  colors: ['#696CFF'],
});
const growthChartSeries = ref([78]);

// Total Revenue Chart (Bar Stacked)
const revenueChartOptions = ref({
  chart: {
    type: 'bar',
    height: 350,
    stacked: true,
  },
  plotOptions: {
    bar: {
      horizontal: false,
      columnWidth: '55%',
      endingShape: 'rounded',
    },
  },
  dataLabels: {
    enabled: false,
  },
  stroke: {
    show: true,
    width: 2,
    colors: ['transparent'],
  },
  xaxis: {
    categories: ['Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct'],
  },
  yaxis: {
    title: {
      text: '$ (thousands)',
    },
  },
  fill: {
    opacity: 1,
  },
  tooltip: {
    y: {
      formatter: function (val) {
        return "$ " + val + " thousands"
      }
    }
  },
  colors: ['#696CFF', '#8592A3'],
});
const revenueChartSeries = ref([
  {
    name: '2023',
    data: [44, 55, 41, 67, 22, 43, 21, 33, 45]
  },
  {
    name: '2024',
    data: [13, 23, 20, 8, 13, 27, 33, 12, 19]
  }
]);

// Profile Report Chart (Line)
const profileChartOptions = ref({
  chart: {
    height: 350,
    type: 'line',
    zoom: {
      enabled: false
    }
  },
  dataLabels: {
    enabled: false
  },
  stroke: {
    curve: 'smooth'
  },
  grid: {
    row: {
      colors: ['#f3f3f3', 'transparent'], // takes 2 values for the alternate background colors
      opacity: 0.5
    },
  },
  xaxis: {
    categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep'],
  },
  colors: ['#FFAB00'],
});
const profileChartSeries = ref([
  {
    name: 'Profile Views',
    data: [10, 41, 35, 51, 49, 62, 69, 91, 148]
  }
]);

// Order Statistics Chart (Pie)
const orderChartOptions = ref({
  chart: {
    width: 380,
    type: 'pie',
  },
  labels: ['Electronic', 'Sports', 'Decor', 'Fashion'],
  responsive: [{
    breakpoint: 480,
    options: {
      chart: {
        width: 200
      },
      legend: {
        position: 'bottom'
      }
    }
  }],
  colors: ['#696CFF', '#8592A3', '#71DD37', '#FF3E1D'],
});
const orderChartSeries = ref([44, 55, 13, 43]);

</script>

<style>
.bg-gradient-primary {
  background: linear-gradient(45deg, #696CFF, #8592A3);
  color: white;
}
</style>
