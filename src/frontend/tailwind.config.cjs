module.exports = {
  content: ["./index.html", "./src/**/*.{ts,tsx,js,jsx}"],
  theme: {
    extend: {
      colors: {
        surface: '#10141a',
        'surface-container-lowest': '#0a0e14',
        'surface-container-low': '#161b22',
        'surface-container': '#1c2128',
        'surface-container-high': '#262a31',
        'surface-container-highest': '#2e3440',
        'on-surface': '#e6edf3',
        'on-surface-variant': '#8b949e',
        primary: '#acc7ff',
        'primary-container': '#498fff',
        'on-primary': '#001a41',
        'on-primary-container': '#e8f1ff',
        'outline-variant': '#30363d',
        'secondary-container': '#243447',
        'on-secondary-container': '#acc7ff',
        tertiary: '#ffb68b',
        'tertiary-container': '#4a2800',
        'error': '#f85149',
        'success': '#3fb950',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      borderRadius: {
        sm: '0.125rem',
        md: '0.375rem',
        DEFAULT: '0.375rem',
        lg: '0.5rem',
        xl: '0.75rem',
      },
      backgroundImage: {
        'primary-gradient': 'linear-gradient(135deg, #acc7ff 0%, #498fff 100%)',
      },
      boxShadow: {
        ambient: '0 0 40px 0 rgba(230,237,243,0.04)',
        glow: '0 0 10px 0 rgba(172,199,255,0.10)',
      },
    },
  },
  plugins: [],
}

