<template>
  <v-form @submit.prevent="handleRegister">
    <v-text-field
      v-model="username"
      label="Username"
      :rules="[rules.required]"
    ></v-text-field>

    <v-text-field
      v-model="email"
      label="Email"
      type="email"
      :rules="[rules.required, rules.email]"
    ></v-text-field>

    <v-text-field
      v-model="password"
      label="Password"
      :type="showPassword ? 'text' : 'password'"
      :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
      @click:append-inner="showPassword = !showPassword"
      :rules="[rules.required]"
    ></v-text-field>

    <v-checkbox v-model="agreeToTerms" :rules="[rules.required]">
      <template #label>
        <div>
          I agree to
          <a href="#" @click.prevent>privacy policy & terms</a>
        </div>
      </template>
    </v-checkbox>

    <v-btn type="submit" block color="primary" class="mt-4">Sign Up</v-btn>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const username = ref('');
const email = ref('');
const password = ref('');
const agreeToTerms = ref(false);
const showPassword = ref(false);
const router = useRouter();

const rules = {
  required: (value: any) => !!value || 'Required.',
  email: (value: string) => /.+@.+\..+/.test(value) || 'E-mail must be valid.',
};

const emit = defineEmits(['register-success', 'register-fail']);

const handleRegister = () => {
  if (username.value && email.value && password.value && agreeToTerms.value) {
    emit('register-success');
    router.push('/dashboard');
  } else {
    emit('register-fail', 'Please fill in all fields');
  }
};
</script>