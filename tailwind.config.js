/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.{html,cshtml}",
    "./Pages/**/*.{html,cshtml}",
    "./wwwroot/**/*.{html,js}"
  ],
  safelist: [
    'bg-tanzanian-green',
    'bg-tanzanian-blue', 
    'bg-uae-red',
    'hover:bg-tanzanian-green',
    'hover:bg-tanzanian-blue',
    'hover:bg-uae-red'
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#fff7ed',
          100: '#ffedd5',
          200: '#fed7aa',
          300: '#fdba74',
          400: '#fb923c',
          500: '#f97316',
          600: '#ea580c',
          700: '#c2410c',
          800: '#9a3412',
          900: '#7c2d12',
        },
        'tanzanian-green': '#00B04F',
        'tanzanian-blue': '#0F4A6C',
        'uae-red': '#CE1126'
      },
      fontFamily: {
        'sans': ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        'inter': ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      }
    },
  },
  plugins: [],
}
