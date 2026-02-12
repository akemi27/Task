import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx' // 👈 Volvemos a importar App
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App /> {/* 👈 Renderizamos App */}
  </React.StrictMode>,
)