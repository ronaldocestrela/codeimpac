import React from 'react'
import { Routes, Route, Link } from 'react-router-dom'

function Home() {
  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold">CodeImpact Frontend</h1>
      <p className="mt-2">Welcome — scaffolded app.</p>
      <Link to="/login" className="text-blue-600">Login</Link>
    </div>
  )
}

function Login() {
  return (
    <div className="p-4">
      <h2 className="text-xl">Login (placeholder)</h2>
    </div>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/login" element={<Login />} />
    </Routes>
  )
}
