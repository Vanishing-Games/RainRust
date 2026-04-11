import { defineConfig } from 'vitepress'

export default defineConfig({
  title: "RainRust",
  description: "RainRust Game Documentation",
  base: '/RainRust/',
  themeConfig: {
    nav: [
      { text: 'Home', link: '/' }
    ],
    socialLinks: [
      { icon: 'github', link: 'https://github.com/VanishXiao/RainRust' }
    ]
  }
})
