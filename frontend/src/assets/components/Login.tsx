import { useState } from 'react';

// Definimos qué propiedades recibe este componente
interface LoginProps {
  onLoginSuccess: (usuario: {nombre: string, id: number}) => void;
}

function Login({ onLoginSuccess }: LoginProps) {
  const [usuario, setUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const manejarLogin = () => {
    // Limpiamos errores previos
    setError("");

    // Hacemos la petición a tu API .NET
    fetch("http://localhost:5102/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ nombreUsuario: usuario, password: password })
    })
    .then(async (response) => {
      if (response.ok) {
        // ¡LOGIN EXITOSO! (Código 200)
        const data = await response.json();
        onLoginSuccess({nombre: data.usuario, id: data.id}); // Avisamos al padre que entramos
      } else {
        // ¡LOGIN FALLIDO! (Código 401)
        setError("Usuario o contraseña incorrectos 🚫");
      }
    })
    .catch(() => setError("Error de conexión con el servidor 🔌"));
  };

  return (
    <div className="min-h-screen bg-[#FFFBEB] flex items-center justify-center relative overflow-hidden">
      
      {/* Círculo Azul */}
      <div className="absolute -bottom-20 -left-20 w-64 h-64 bg-blue-300 rounded-full opacity-80"></div>
      
      {/* Rombo Amarillo */}
      <div className="absolute -top-20 -right-20 w-80 h-80 bg-yellow-200 rotate-45 rounded-3xl opacity-90"></div>

      {/* Tarjeta */}
      <div className="relative z-10 bg-sky-300 p-8 rounded-3xl shadow-xl w-full max-w-sm flex flex-col gap-6">
        
        <h2 className="text-3xl font-bold text-orange-500 text-center" style={{ fontFamily: 'cursive' }}>
          Bienvenido
        </h2>

        {/* Mensaje de Error (Solo sale si falla) */}
        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-2 rounded relative">
            {error}
          </div>
        )}

        <div className="flex flex-col gap-1">
          <label className="text-orange-500 font-semibold ml-1">Usuario</label>
          <input 
            type="text" 
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            className="p-2 rounded-lg border-2 border-blue-400 focus:outline-none focus:border-orange-400"
          />
        </div>

        <div className="flex flex-col gap-1">
          <label className="text-orange-500 font-semibold ml-1">Contraseña</label>
          <input 
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)} 
            className="p-2 rounded-lg border-2 border-blue-400 focus:outline-none focus:border-orange-400"
          />
        </div>

        <button 
          onClick={manejarLogin}
          className="bg-orange-500 text-white font-bold py-2 rounded-xl hover:bg-orange-600 transition-colors shadow-md"
        >
          Ingresar
        </button>
      </div>
    </div>
  );
}

export default Login;