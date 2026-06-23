<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { listar, eliminarTransaccion, editarTransaccion } from '../services/tranService'


const router = useRouter()
const lista = ref([])
const cargando = ref(true)
const error = ref(null)
const filaExpandida = ref(null)
const mostrarModalEditar = ref(false)
const cargandoEdicion = ref(false)

const formularioEdicion = ref({
  id: null,
  operacion: '',
  moneda: '',
  valor: '',
  monto: '',
  usuarioId: null,
  fecha: ''
})

const monedas = [
  { nombre: 'Bitcoin', clave: 'BITCOIN' },
  { nombre: 'Ethereum', clave: 'ETHEREUM' },
  { nombre: 'USDC', clave: 'USDC' },
  { nombre: 'Litecoin', clave: 'LITECOIN' },
  { nombre: 'Dogecoin', clave: 'DOGECOIN' }
]

const operaciones = ['Compra', 'Venta']

onMounted(async () => {
  try {
    const response = await listar()
    lista.value = response
    console.log(response)
  } catch (err) {
    console.error(err)
    error.value = 'Error al cargar transacciones'
  } finally {
    cargando.value = false
  }
})

function expandirFila(id) {
  filaExpandida.value = filaExpandida.value === id ? null : id
}

function abrirModalEditar(transaccion) {
  formularioEdicion.value = {
    id: transaccion.id,
    operacion: transaccion.operacion,
    moneda: transaccion.moneda,
    valor: transaccion.valor,
    monto: transaccion.monto,
    usuarioId: transaccion.usuarioId,
    fecha: transaccion.fecha
  }
  mostrarModalEditar.value = true
}

function cerrarModalEditar() {
  mostrarModalEditar.value = false
  formularioEdicion.value = {
    id: null,
    operacion: '',
    moneda: '',
    valor: '',
    monto: '',
    usuarioId: null,
    fecha: ''
  }
}

async function guardarEdicion() {
  if (!formularioEdicion.value.operacion || !formularioEdicion.value.moneda || !formularioEdicion.value.valor || !formularioEdicion.value.monto) {
    alert('Por favor completa todos los campos')
    return
  }

 try {
    cargandoEdicion.value = true
    
    // Crear el objeto correctamente
    const transaccionActualizada = {
      id: formularioEdicion.value.id,
      usuarioId: formularioEdicion.value.usuarioId,
      operacion: formularioEdicion.value.operacion,
      moneda: formularioEdicion.value.moneda,
      monto: parseFloat(formularioEdicion.value.monto),
      valor: parseFloat(formularioEdicion.value.valor),
      fecha: formularioEdicion.value.fecha
    }

    console.log('Guardando edición con ID:', formularioEdicion.value.id)
    console.log('Datos a enviar:', transaccionActualizada)

    const response = await editarTransaccion(
      transaccionActualizada
    )

    console.log('Respuesta del servidor:', response)

    if (response.error) {
      alert('Error: ' + response.error)
      return
    }

    if (response.success) {
      // Actualizar la lista
      const listActualizada = await listar()
      lista.value = listActualizada
      
      alert('Transacción editada con éxito')
      cerrarModalEditar()
      filaExpandida.value = null
    }
  } catch (err) {
    console.error('Error:', err)
    alert('Error al editar la transacción: ' + err.message)
  } finally {
    cargandoEdicion.value = false
  }
}

function borrarTransaccion(id) {
  if (confirm('¿Estás seguro de que deseas eliminar esta transacción?')) {
    console.log('Borrar:', id)
    eliminarTransaccion(id)
      .then(() => {
        lista.value = lista.value.filter(t => t.id !== id)
      })
      .catch(err => {
        console.error(err)
        alert('Error al eliminar la transacción')
      })
  }
}
</script>

<template>
  <div>
    <h1 class="title">Whalet</h1>

     <div v-if="cargando" class="cargando">
      <p>Cargando transacciones...</p>
    </div>

    <div v-else-if="error" class="error">
      <p>{{ error }}</p>
    </div>

  <!-- Aquí iría la lógica para mostrar las transacciones del usuario -->
 <div v-if="lista.length > 0" class="tabla-container">
      <table class="tabla-transacciones">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Monto</th>
            <th>Tipo</th>
            <th>Moneda</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <template v-for="transaccion in lista" :key="transaccion.id">
              <tr 
                class="fila-principal"
                @click="expandirFila(transaccion.id)"
                @click.self="expandirFila(null)"
                :class="{ activa: filaExpandida === transaccion.id }"
              >
                <td>{{ new Date(transaccion.fecha).toLocaleDateString('es-AR') }}</td>
                <td :class="transaccion.operacion === 'Venta' ? 'monto-positivo' : 'monto-negativo'">
                  {{ transaccion.monto }}
                </td>
                <td>
                  <span :class="['badge', transaccion.operacion.toLowerCase()]">
                    {{ transaccion.operacion }}
                  </span>
                </td>
                <td>{{ transaccion.moneda }}</td>
                <td>
                  <span v-if="filaExpandida === transaccion.id" class="indicador">▼</span>
                  <span v-else class="indicador">▶</span>
                </td>
              </tr>

              <!-- Fila expandida con detalles -->
              <tr v-if="filaExpandida === transaccion.id" class="fila-expandida">
                <td colspan="5">
                  <div class="detalles-contenedor">
                    <div class="detalles-grid">
                      <div class="detalle">
                        <label>Valor Total:</label>
                        <p>${{ transaccion.valor?.toLocaleString('es-AR') || 'N/A' }} ARS</p>
                      </div>
                      <div class="detalle">
                        <label>ID Transacción:</label>
                        <p>{{ transaccion.id }}</p>
                      </div>
                      <div class="detalle">
                        <label>Usuario ID:</label>
                        <p>{{ transaccion.usuarioId }}</p>
                      </div>
                      <div class="detalle">
                        <label>Hora:</label>
                        <p>{{ new Date(transaccion.fecha).toLocaleTimeString('es-AR') }}</p>
                      </div>
                    </div>

                    <div class="botones-acciones">
                      <button 
                        @click="abrirModalEditar(transaccion)"
                        class="btn-editar"
                      >
                        ✏️ Editar
                      </button>
                      <button 
                        @click="borrarTransaccion(transaccion.id)"
                        class="btn-borrar"
                      >
                        🗑️ Borrar
                      </button>
                    </div>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

    <div v-else class="sin-transacciones">
      <p>No hay transacciones registradas</p>
    </div>
  </div> 

    <div v-if="mostrarModalEditar" class="modal-overlay" @click.self="cerrarModalEditar">
      <div class="modal">
        <div class="modal-header">
          <h2>Editar Transacción</h2>
          <button class="btn-cerrar" @click="cerrarModalEditar">&times;</button>
        </div>

        <div class="modal-body">
          <div class="formulario-grupo">
            <label>Operación:</label>
            <select v-model="formularioEdicion.operacion">
              <option v-for="op in operaciones" :key="op" :value="op">
                {{ op }}
              </option>
            </select>
          </div>

          <div class="formulario-grupo">
            <label>Moneda:</label>
            <select v-model="formularioEdicion.moneda">
              <option v-for="moneda in monedas" :key="moneda.clave" :value="moneda.clave">
                {{ moneda.nombre }}
              </option>
            </select>
          </div>

          <div class="formulario-grupo">
            <label>Monto (cantidad de criptomoneda):</label>
            <input
              v-model.number="formularioEdicion.monto"
              type="number"
              placeholder="0.00"
              step="0.00000001"
            >
          </div>

          <div class="formulario-grupo">
            <label>Valor (en ARS):</label>
            <input
              v-model.number="formularioEdicion.valor"
              type="number"
              placeholder="0.00"
              step="0.01"
            >
          </div>

          <div class="info-resumen">
            <p><strong>ID Transacción:</strong> {{ formularioEdicion.id }}</p>
            <p><strong>Usuario ID:</strong> {{ formularioEdicion.usuarioId }}</p>
            <p><strong>Fecha:</strong> {{ new Date(formularioEdicion.fecha).toLocaleString('es-AR') }}</p>
          </div>
        </div>

        <div class="modal-footer">
          <button @click="cerrarModalEditar" class="btn-cancelar">
            Cancelar
          </button>
          <button 
            @click="guardarEdicion" 
            class="btn-guardar"
            :disabled="cargandoEdicion"
          >
            {{ cargandoEdicion ? 'Guardando...' : 'Guardar Cambios' }}
          </button>
        </div>
      </div>
    </div>
</template>

<style scoped>
.contenedor {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.title {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-size: 3rem;
  font-weight: bold;
  color: rgb(0, 132, 255);
  margin-bottom: 2rem;
}

.cargando, .error {
  text-align: center;
  padding: 2rem;
  font-size: 1.1rem;
}

.error {
  color: red;
  background: #ffebee;
  border-radius: 8px;
  border: 1px solid #ef5350;
  padding: 1rem;
}

.tabla-container {
  overflow-x: auto;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.tabla-transacciones {
  width: 100%;
  border-collapse: collapse;
  background: #1a2c64;
}

.tabla-transacciones th {
  background-color: rgb(0, 132, 255);
  color: rgb(255, 255, 255);
  padding: 12px;
  text-align: left;
  font-weight: bold;
}

.fila-principal {
  cursor: pointer;
  transition: all 0.3s ease;
}

.fila-principal:hover {
  background-color: rgb(0, 132, 255);
}

.fila-principal.activa {
  background-color: #1a2c64;
}

.tabla-transacciones td {
  padding: 12px;
  border-bottom: 1px solid #ddd;
}

.monto-positivo {
  color: #0ae915;
  font-weight: bold;
}

.monto-negativo {
  color: #ff0000;
  font-weight: bold;
}

.badge {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: bold;
}

.badge.compra {
  background-color: #e3f2fd;
  color: rgb(0, 132, 255);
}

.badge.venta {
  background-color: #fff3e0;
  color: #e65100;
}

.indicador {
  color: rgb(0, 132, 255);
  font-weight: bold;
}

.fila-expandida {
  background-color: #f5f5f5;
}

.detalles-contenedor {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 20px;
}

.detalles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  background: white;
  padding: 20px;
  border-radius: 8px;
  border-left: 4px solid rgb(0, 132, 255);
}

.detalle label {
  display: block;
  font-weight: bold;
  color: rgb(0, 132, 255);
  font-size: 0.85rem;
  margin-bottom: 5px;
}

.detalle p {
  margin: 0;
  color: #333;
  word-break: break-all;
}

.botones-acciones {
  display: flex;
  gap: 10px;
}

.btn-editar,
.btn-borrar {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-editar {
  background-color: rgb(0, 132, 255);
  color: white;
}

.btn-editar:hover {
  background-color: rgb(0, 100, 200);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 132, 255, 0.3);
}

.btn-borrar {
  background-color: #ef5350;
  color: white;
}

.btn-borrar:hover {
  background-color: #c62828;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(239, 83, 80, 0.3);
}

.sin-transacciones {
  text-align: center;
  padding: 3rem;
  color: #999;
  background: #f5f5f5;
  border-radius: 8px;
}

/* MODAL STYLES */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
  width: 90%;
  max-width: 500px;
  animation: slideIn 0.3s ease;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-50px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #eee;
}

.modal-header h2 {
  margin: 0;
  color: rgb(0, 132, 255);
  font-size: 1.5rem;
}

.btn-cerrar {
  background: none;
  border: none;
  font-size: 2rem;
  cursor: pointer;
  color: #666;
  transition: color 0.3s;
}

.btn-cerrar:hover {
  color: #000;
}

.modal-body {
  padding: 20px;
  max-height: 60vh;
  overflow-y: auto;
}

.formulario-grupo {
  margin-bottom: 20px;
}

.formulario-grupo label {
  display: block;
  margin-bottom: 8px;
  font-weight: bold;
  color: #333;
  font-size: 0.95rem;
}

.formulario-grupo input,
.formulario-grupo select {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  box-sizing: border-box;
}

.formulario-grupo input:focus,
.formulario-grupo select:focus {
  outline: none;
  border-color: rgb(0, 132, 255);
  box-shadow: 0 0 0 3px rgba(0, 132, 255, 0.1);
}

.info-resumen {
  background: #f0f8ff;
  padding: 15px;
  border-radius: 6px;
  border-left: 4px solid rgb(0, 132, 255);
  margin-bottom: 20px;
}

.info-resumen p {
  margin: 8px 0;
  color: #333;
  font-size: 0.9rem;
}

.modal-footer {
  display: flex;
  gap: 10px;
  padding: 20px;
  border-top: 1px solid #eee;
}

.btn-cancelar,
.btn-guardar {
  flex: 1;
  padding: 12px;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-cancelar {
  background: #f0f0f0;
  color: #333;
}

.btn-cancelar:hover {
  background: #e0e0e0;
}

.btn-guardar {
  background: rgb(0, 132, 255);
  color: white;
}

.btn-guardar:hover:not(:disabled) {
  background: rgb(0, 100, 200);
}

.btn-guardar:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .modal {
    width: 95%;
  }

  .detalles-grid {
    grid-template-columns: 1fr;
  }
}
</style>