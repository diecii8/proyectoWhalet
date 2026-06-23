export async function login(nombre, contraseña) {

    const respuesta = await fetch(
        'https://localhost:7251/api/Usuarios/validarUsuario',
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                nombre,
                contraseña
            })
        }
    );

    const resultado = await respuesta.json();

    return resultado;
}

export async function signUp(mail, nombre, contraseña) {

    const respuesta = await fetch(
        'https://localhost:7251/api/Usuarios/crearUsuario',
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                mail,
                nombre,
                contraseña,
                tipo: 'usuario'
            })
        }
    );

    const resultado = await respuesta.json();

    return resultado;
}