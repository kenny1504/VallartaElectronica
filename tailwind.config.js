/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Controllers/**/*.cs",
    "./ViewModels/**/*.cs",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        tinta: "#14324a",
        panel: "#ffffff",
        panel2: "#f8fbff",
        borde: "#d6e4f4",
        acento: "#e12b2d",
        acento2: "#3399ea",
        exito: "#19a974",
        rojoMarca: "#df2027",
        amarilloMarca: "#ffbe18",
        azulMarca: "#3aa0f3",
        azulClaroMarca: "#eaf6ff",
        cieloMarca: "#f4fbff"
      },
      boxShadow: {
        panel: "0 18px 48px rgba(28, 74, 118, 0.14)"
      },
      borderRadius: {
        "4xl": "2rem"
      }
    }
  },
  plugins: []
};
