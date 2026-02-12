import { Trash2, CheckCircle, Circle, Calendar } from "lucide-react";

interface Tarea {
  id: number;
  nombre: string;
  completada: boolean;
  fechaVencimiento?: string;
}

interface TaskCardProps {
  tarea: Tarea;
  onToggle: (tarea: Tarea) => void;
  onDelete: (id: number) => void;
}

export function TaskCard({ tarea, onToggle, onDelete }: TaskCardProps) {
  const fechaDate = tarea.fechaVencimiento
    ? new Date(tarea.fechaVencimiento)
    : null;
  const hoy = new Date();
  hoy.setHours(0,0,0,0);
  const fechaVencio = fechaDate && fechaDate < hoy && !tarea.completada;
  const fechaHoy = fechaDate && fechaDate.getDate() == hoy.getDate() && !tarea.completada;
  return (
    <div
      className={`relative group p-6 rounded-2xl border-2 transition-all duration-300 hover:-translate-y-1 hover:shadow-xl bg-white ${tarea.completada ? "border-green-400 opacity-60" : "border-transparent"}`}
    >
      <h3
        className={`font-bold text-lg mb-4 text-gray-700 ${tarea.completada ? "line-through" : ""}`}
      >
        {tarea.nombre}
      </h3>
      {fechaDate && (
        <div
          className={`flex items-center gap-1 text-xs font-bold px-2 py-1 rounded-md w-fit mb-4 ${
            fechaVencio
              ? "bg-red-100 text-red-600" 
              : fechaHoy 
              ? "bg-blue-100 text-blue-500"   
              :"bg-gray-100 text-gray-500" 
          }`}
        >
          <Calendar size={14} />
          <span>{fechaDate.toLocaleDateString()}</span>
          {fechaVencio && <span>⚠️ Vencida</span>}
          {fechaHoy && <span> Es hoy</span>}
        </div>
      )}
      <div className="flex justify-end gap-2 mt-2">
        <button
          onClick={() => onToggle(tarea)}
          className={`${tarea.completada ? "text-green-600" : "text-gray-300 hover:text-blue-500"}`}
        >
          {tarea.completada ? <CheckCircle /> : <Circle />}
        </button>
        <button
          onClick={() => onDelete(tarea.id)}
          className="text-gray-300 hover:text-red-500 transition-colors"
        >
          <Trash2 />
        </button>
      </div>
    </div>
  );
}
