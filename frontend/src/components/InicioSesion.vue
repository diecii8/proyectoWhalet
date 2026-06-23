<script setup>
import { ref } from 'vue'
import { login, signUp } from '../services/authService'
import {crearBilletera} from '../services/walletService'
import { useRouter } from 'vue-router'

const router = useRouter()

const mail = ref('')
const nombre = ref('')
const contraseña = ref('')

// Modal de registro
const mostrarModalRegistro = ref(false)
const formularioRegistro = ref({
  nombre: '',
  email: '',
  contraseña: '',
  confirmacion: ''
})
const mensajeRegistro = ref(null)
const cargandoRegistro = ref(false)

const iniciarSesion = async () => {
    try {

        const resultado = await login(
            nombre.value,
            contraseña.value
        );

        console.log(resultado);

        if (resultado.estado === 'ok') {
            alert('Bienvenido ' + resultado.usuario.nombre);
            localStorage.setItem('user', JSON.stringify(resultado.usuario));
            await crearBilletera(resultado.usuario.id);
            router.push('/BilleteraUsuario')
        } else {
            alert(resultado.mensaje);
        }

    } catch (error) {
        console.error(error);
        alert('Ocurrió un error al iniciar sesión');
    }
}

function abrirModalRegistro() {
  mostrarModalRegistro.value = true
}

function cerrarModalRegistro() {
  mostrarModalRegistro.value = false
  formularioRegistro.value = {
    nombre: '',
    email: '',
    contraseña: '',
    confirmacion: ''
  }
  mensajeRegistro.value = null
}

async function registrarUsuario() {
  // Validaciones
  if (!formularioRegistro.value.nombre || formularioRegistro.value.nombre.trim() === '') {
    mensajeRegistro.value = { tipo: 'error', texto: 'El nombre es requerido' }
    return
  }

  if (!formularioRegistro.value.email || formularioRegistro.value.email.trim() === '') {
    mensajeRegistro.value = { tipo: 'error', texto: 'El email es requerido' }
    return
  }

  if (!formularioRegistro.value.contraseña || formularioRegistro.value.contraseña.trim() === '') {
    mensajeRegistro.value = { tipo: 'error', texto: 'La contraseña es requerida' }
    return
  }

  if (formularioRegistro.value.contraseña !== formularioRegistro.value.confirmacion) {
    mensajeRegistro.value = { tipo: 'error', texto: 'Las contraseñas no coinciden' }
    return
  }

  if (formularioRegistro.value.contraseña.length < 6) {
    mensajeRegistro.value = { tipo: 'error', texto: 'La contraseña debe tener al menos 6 caracteres' }
    return
  }

  try {
    cargandoRegistro.value = true
    
    const respuesta = await signUp(
      formularioRegistro.value.email,
      formularioRegistro.value.nombre,
      formularioRegistro.value.contraseña
    )

    if (respuesta.estado === 'ok' || respuesta.success) {
      mensajeRegistro.value = { 
        tipo: 'exito', 
        texto: 'Cuenta creada exitosamente. Intenta iniciar sesión.' 
      }
      
      // Limpiar formulario de login
      nombre.value = formularioRegistro.value.nombre
      contraseña.value = formularioRegistro.value.contraseña
      
      setTimeout(() => {
        cerrarModalRegistro()
      }, 2000)
    } else {
      mensajeRegistro.value = { 
        tipo: 'error', 
        texto: respuesta.mensaje || 'Error al crear la cuenta' 
      }
    }
  } catch (error) {
    console.error('Error al registrar:', error)
    mensajeRegistro.value = { 
      tipo: 'error', 
      texto: 'Error al crear la cuenta. Intenta de nuevo.' 
    }
  } finally {
    cargandoRegistro.value = false
  }
}

</script> 

<template>
  <div>
    <h1 class="title">Whalet</h1>

    <ul>   
      <li>
        <input
        type="text"
        v-model="nombre"
        placeholder="Nickname"
        >
      </li>
      <li>
        <input
        type="password"
        v-model="contraseña"
        placeholder="Contraseña"
        >
      </li>
    </ul>
    <button @click="iniciarSesion" class="btn-iniciar">
      Iniciar sesión
    </button>

    <button @click="abrirModalRegistro" class="btn-registro">
      Crear cuenta
    </button>

    <!-- Modal de Registro -->
    <div v-if="mostrarModalRegistro" class="modal-overlay" @click.self="cerrarModalRegistro">
      <div class="modal">
        <div class="modal-header">
          <h2>Crear Nueva Cuenta</h2>
          <button class="btn-cerrar" @click="cerrarModalRegistro">&times;</button>
        </div>

        <div class="modal-body">
          <!-- Mensaje de estado -->
          <div v-if="mensajeRegistro" :class="['mensaje', mensajeRegistro.tipo]">
            {{ mensajeRegistro.texto }}
          </div>

          <!-- Formulario -->
          <div class="formulario-grupo">
            <label>Nombre de usuario:</label>
            <input
              v-model="formularioRegistro.nombre"
              type="text"
              placeholder="Tu nombre de usuario"
            >
          </div>

          <div class="formulario-grupo">
            <label>Email:</label>
            <input
              v-model="formularioRegistro.email"
              type="email"
              placeholder="tu@email.com"
            >
          </div>

          <div class="formulario-grupo">
            <label>Contraseña:</label>
            <input
              v-model="formularioRegistro.contraseña"
              type="password"
              placeholder="Mínimo 6 caracteres"
            >
          </div>

          <div class="formulario-grupo">
            <label>Confirmar contraseña:</label>
            <input
              v-model="formularioRegistro.confirmacion"
              type="password"
              placeholder="Repite tu contraseña"
            >
          </div>
        </div>

        <div class="modal-footer">
          <button @click="cerrarModalRegistro" class="btn-cancelar">
            Cancelar
          </button>
          <button 
            @click="registrarUsuario" 
            class="btn-registrar"
            :disabled="cargandoRegistro"
          >
            {{ cargandoRegistro ? 'Registrando...' : 'Registrarse' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
.title {
  font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-size: 3rem;
  font-weight: bold;
  color: rgb(0, 132, 255);
  margin-bottom: 2rem;
}

.btn-iniciar, .btn-registro {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin: 0.5rem;
  transition: background-color 0.3s;
}

.btn-iniciar {
  background-color: rgb(0, 132, 255);
  color: white;
}

.btn-iniciar:hover {
  background-color: rgb(0, 110, 215);
}

.btn-registro {
  background-color: rgb(40, 167, 69);
  color: white;
}

.btn-registro:hover {
  background-color: rgb(32, 135, 55);
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  color: rgb(0, 132, 255);
}

.btn-cerrar {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #666;
}

.btn-cerrar:hover {
  color: #000;
}

.modal-body {
  padding: 1.5rem;
}

.formulario-grupo {
  margin-bottom: 1.5rem;
}

.formulario-grupo label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

.formulario-grupo input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

.formulario-grupo input:focus {
  outline: none;
  border-color: rgb(0, 132, 255);
  box-shadow: 0 0 0 3px rgba(0, 132, 255, 0.1);
}

.mensaje {
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-weight: 500;
}

.mensaje.error {
  background-color: #ffebee;
  color: #c62828;
  border-left: 4px solid #c62828;
}

.mensaje.exito {
  background-color: #e8f5e9;
  color: #2e7d32;
  border-left: 4px solid #2e7d32;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem;
  border-top: 1px solid #e0e0e0;
}

.btn-cancelar, .btn-registrar {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-cancelar {
  background-color: #f0f0f0;
  color: #333;
}

.btn-cancelar:hover {
  background-color: #e0e0e0;
}

.btn-registrar {
  background-color: rgb(40, 167, 69);
  color: white;
}

.btn-registrar:hover:not(:disabled) {
  background-color: rgb(32, 135, 55);
}

.btn-registrar:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>