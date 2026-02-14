import { useState, useEffect, useCallback } from "react";
import Login from "./assets/components/Login"; // Importamos el Login
import './App.css';
import { Layout } from "./assets/components/Layout";
import { Plus} from 'lucide-react';
import { TaskCard } from "./assets/components/TaskCard";

// Interfaces (Modelos)
interface Tarea {
  id: number;
  nombre: string;
  completada: boolean;
  fechaVencimiento?: string;
}

function App() {

  const [usuarioLogueado, setUsuarioLogueado] = useState<{nombre: string, id: number} | null>(null);

  // ESTADOS DE LA APP DE TAREAS
  const [tareas, setTareas] = useState<Tarea[]>([]);
  const [nuevoTexto, setNuevoTexto] = useState("");
  const [nuevaFecha, setNuevaFecha] = useState("");
  const [filtro, setFiltro] = useState('todas');
  const API_URL = "https://task-8wyw.onrender.com/tareas";

  // --- LÓGICA DE TAREAS (Igual que antes) ---
  const cargarTareas = useCallback(() => {
    if(!usuarioLogueado) return;

    const urlConId = `${API_URL}?usuarioId=${usuarioLogueado.id}`

    fetch(urlConId)
      .then(res => res.json())
      .then(data => setTareas(data))
      .catch(error => console.error("Error conectando:", error));
  }, [usuarioLogueado]);

  useEffect(() => {
    if (usuarioLogueado) {
      cargarTareas();
    }
  }, [usuarioLogueado, cargarTareas]);

  const crearTarea = () => {
    if (!nuevoTexto) return;
    if(!usuarioLogueado) return;

    const nuevaTarea = { nombre: nuevoTexto, completada: false, usuarioId: usuarioLogueado.id, fechaVencimiento: nuevaFecha ? nuevaFecha : null};
    fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(nuevaTarea)
    }).then(() => {
      setNuevoTexto("");
      setNuevaFecha("");
      cargarTareas();
    });
  }

  const toggleTarea = (tarea: Tarea) => {
    const tareaModificada = { ...tarea, completada: !tarea.completada };
    if (!usuarioLogueado) return
    fetch(`${API_URL}/${tarea.id}?usuarioId=${usuarioLogueado.id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(tareaModificada)
    }).then(() => cargarTareas());
  };

  const borrarTarea = (id: number) => {
    if (!usuarioLogueado) return
    fetch(`${API_URL}/${id}?usuarioId=${usuarioLogueado.id}`, { method: "DELETE" })
      .then(() => cargarTareas());
  }

  const cerrarSesion = () => {
    setUsuarioLogueado(null);
    setTareas([]); // Limpiamos la vista por seguridad
  }

  //Logica de filtros
  const tareasFiltradas = tareas.filter((t) =>{
    if(filtro === 'todos') return true

    if(filtro === 'completadas') return t.completada

    if(filtro === 'pendientes') return !t.completada

    if(filtro === 'vencidas') {
      if(!t.fechaVencimiento) return false 

      const hoy = new Date();
      hoy.setHours(0,0,0,0);
      const fechaLimpia = t.fechaVencimiento.substring(0, 10); 
      const fechaTarea = new Date(fechaLimpia + 'T00:00:00');

      return fechaTarea < hoy && !t.completada
    }
    return true
  });

  // --- RENDERIZADO CONDICIONAL ---

  // 1. Si NO hay usuario, mostramos el Login
  if (!usuarioLogueado) {
    return <Login onLoginSuccess={(datos) => setUsuarioLogueado(datos)} />;
  }

  // 2. Si SÍ hay usuario, mostramos la App de Tareas
  return (
    <Layout
      usuario={usuarioLogueado.nombre}
      onLogout={cerrarSesion}  
    >
      <div className="mb-8 flex gap-4">
        <input 
        type="text"
        value={nuevoTexto}
        onChange={(e) => setNuevoTexto(e.target.value)}
        placeholder="¿Qué vamos hacer hoy?"
        className="flex-1 p-4 rounded-xl border-none shadow-sm bg-white focus:ring-2 focus:ring-blue-200 outline-none"
        onKeyDown={(e) => e.key === 'Enter' && crearTarea()} 
        />
        <input 
        type= "date"
        value={nuevaFecha}
        onChange={(f) => setNuevaFecha(f.target.value)}
        className="p-3 bg-gray-50 rounded-xl text-gray-600 outline-none cursor-pointer hover:bg-gray-100 transition-colors"
        />
        <button
          onClick={crearTarea}
          className="bg-blue-600 hover:bg-blue-700 rounded-xl border-none p-4 text-white shadow-lg transition-transform hover:scale-105 flex items-center justify-center"
        >
          <Plus size={24}/>
        </button>
      </div>
      <div className="flex gap-2 mb-6 overflow-x-auto pb-2">
      {['todos','completadas','pendientes','vencidas'].map((f) => (
        <button
          key={f}
          onClick={() => setFiltro(f)}
          className={`px-4 py-2 rounded-full text-sm font-bold capitalize transition-all ${
            filtro === f
            ?'bg-slate-800 text-white shadow-md transform scale-105'
            :'bg-white text-gray-500 hover:bg-gray-100'
          }`}
        >
          {f}
        </button>
      ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {tareasFiltradas.length === 0 && (
          <div className="text-gray-500 col-span-full text-center py-1">
            <p>No hay tareas pendientes</p>
          </div>
        )}
        {tareasFiltradas.map((t => (
          <TaskCard
          key={t.id}
          tarea={t}
          onDelete={borrarTarea}
          onToggle={toggleTarea}
          />
        )))}
      </div>
    </Layout>
  );
}

export default App;