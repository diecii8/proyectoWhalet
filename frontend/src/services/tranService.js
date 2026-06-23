export async function listar() {

const usuario = JSON.parse(localStorage.getItem('user'));

const respuesta = await fetch(
    `https://localhost:7251/api/Transactions/obtenerTransacciones/${usuario.id}`,
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

export async function crearTransaccion(transaccion) {
try {
    console.log('URL:', 'https://localhost:7251/api/Transactions/crearTransaccion')
    console.log('Datos enviados:', transaccion)

    const respuesta = await fetch(
      'https://localhost:7251/api/Transactions/crearTransaccion',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          usuarioId: transaccion.usuarioId,
          operacion: transaccion.operacion,
          moneda: transaccion.moneda,
          monto: transaccion.monto,
          valor: transaccion.valor,
          fecha: transaccion.fecha
        })
      }
    );

    console.log('Status:', respuesta.status)

    if (!respuesta.ok) {
      throw new Error(`Error ${respuesta.status}`)
    }

    const resultado = await respuesta.json();


    if (resultado.error) {
      throw new Error(resultado.error)
    }

    return { success: true, data: resultado };
  } catch (error) {
    console.error('Error en crearTransaccion:', error)
    return { error: error.message }
  }
}

export async function eliminarTransaccion(id) {
    try {
        const respuesta = await fetch(
            `https://localhost:7251/api/Transactions/eliminarTransaccion/${id}`,
            {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            }
        );
        if (!respuesta.ok) {
            throw new Error(`Error ${respuesta.status}`)
        }
        const resultado = await respuesta.json();
        if (resultado.error) {
            throw new Error(resultado.error)
        }
        return { success: true, data: resultado };
    } catch (error) {
        console.error('Error en eliminarTransaccion:', error)
        return { error: error.message }
     }
}

export async function editarTransaccion(transaccion) {
    try {
        const id = parseInt(transaccion.id)
        console.log('URL:', `https://localhost:7251/api/Transactions/editarTransaccion/${id}`)
        console.log('Datos enviados:', transaccion)

        const respuesta = await fetch(
            `https://localhost:7251/api/Transactions/editarTransaccion/${id}`,
            {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    usuarioId: transaccion.usuarioId,
                    operacion: transaccion.operacion,
                    moneda: transaccion.moneda,
                    monto: transaccion.monto,
                    valor: transaccion.valor,
                    fecha: transaccion.fecha
                })
            }
        );
        if (!respuesta.ok) {
            throw new Error(`Error ${respuesta.status}`)
        }
        const resultado = await respuesta.json();
        if (resultado.error) {
            throw new Error(resultado.error)
        }
        return { success: true, data: resultado };
    } catch (error) {
        console.error('Error en editarTransaccion:', error)
        return { error: error.message }
        }
}
