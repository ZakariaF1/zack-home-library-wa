/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        ink: "#1c1917",
        paper: "#f5f1ea",
        pine: "#1f4d3a",
        clay: "#b45309",
      },
      fontFamily: {
        display: ['"Source Serif 4"', "Georgia", "serif"],
        sans: ['"IBM Plex Sans"', "Segoe UI", "sans-serif"],
      },
      screens: {
        tablet: "640px",
        desktop: "1024px",
        ultrawide: "1536px",
      },
    },
  },
  plugins: [],
};
