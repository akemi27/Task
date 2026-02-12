import type React from "react";

interface LayoutsProps {
    children: React.ReactNode;
    usuario: string;
    onLogout: () => void;
}

export function Layout({children, usuario, onLogout} : LayoutsProps){
    return (
        <div className="flex h-screen w-full bg-gray-100">
            <aside className="w-64 bg-slate-900 text-white flex flex-col">
                <div className="h-16 flex flex-col items-center justify-center border-b border-slate-500">
                   <h1 className="font-bold text-2xl tracking-wider">Task</h1>
                   <h2 className="font-semibold text-xs tracking-wider">Hola {usuario}</h2>
                </div>
                <nav className="flex-1 px-4 py-6 space-y-2">
                    <div className="px-4 py-2 bg-blue-600 rounded-lg cursor-pointer">Tablero</div>
                    <div className="px-4 py-2 hover:bg-slate-800 rounded-lg cursor-pointer transition-colors">Configuración</div>
                </nav>
                <div className="flex justify-center mb-4">
                    <button
                    onClick={onLogout}
                    className="bg-red-800 px-4 py-2 rounded-lg shadow-lg hover:bg-red-900 hover:scale-105 transition-all duration-150"
                    >
                     Cerrar   
                    </button>
                </div>
            </aside>
            <main className="flex-1 overflow-y-auto p-8 relative">
                {children}
            </main>
        </div>
    )
};