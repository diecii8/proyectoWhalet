export async function crearBilletera(idUsuario) {
 
    const respuesta = await fetch(
        `https://localhost:7251/api/billeteras/crear/${idUsuario}`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                idUsuario
            })
        }
    );
    const resultado = await respuesta.json();
    return resultado;
}

export async function obtenerBilletera(idUsuario) { 
    
    const respuesta = await fetch(
        `https://localhost:7251/api/billeteras/obtener/${idUsuario}`,
        {       
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        }
    );
    const resultado = await respuesta.json();
    return resultado;
}   