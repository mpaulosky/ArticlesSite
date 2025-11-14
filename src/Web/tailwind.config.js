/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class', // Enable class-based dark mode
  content: [
    './Components/**/*.razor',
    './Components/**/*.html',
    './Pages/**/*.razor',
    './Pages/**/*.html'
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
