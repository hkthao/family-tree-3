import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DashboardView from '@/views/DashboardView.vue'
import vuetify from '../plugins/vuetify'

describe('DashboardView.vue', () => {
  it('renders correctly', () => {
    const wrapper = mount(DashboardView, { global: { plugins: [vuetify] } })
    expect(wrapper.text()).toContain('Chào mừng đến với dự án Cây Gia Phả!')
  })
})
