<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { obtenerTasasCambio, obtenerPrecioMoneda } from '../services/apiService'
import { obtenerBilletera} from '../services/walletService'
import { crearTransaccion } from '../services/tranService'

const router = useRouter()
const usuario = ref(null)
const billetera = ref(null)
const cargando = ref(true)
const error = ref(null)
const mostrarModalCompra = ref(false)
const mostrarModalVenta = ref(false)
const cargandoCompra = ref(false)
const cargandoVenta = ref(false)
const mensajeCompra = ref(null)
const mensajeVenta = ref(null)
const cargandoPrecio = ref(false)
const tasasCambio = ref({})


const formularioCompra = ref({  
  moneda: 'bitcoin',
  cantidad: 0,
  monto: 0,
  precioUnitario: 0,
  exchange: 'satoshitango',
})

const formularioVenta = ref({  
  moneda: 'bitcoin',
  cantidad: 0,
  monto: 0,
  precioUnitario: 0,
  exchange: 'satoshitango',
})

const monedas = [
  { nombre: 'Bitcoin', clave: 'BitCoin', label: 'BTC', valor: 'bitcoin', dto: 'bitCoin' },
  { nombre: 'Ethereum', clave: 'Ethereum', label: 'ETH', valor: 'ethereum', dto: 'ethereum' },
  { nombre: 'USDC', clave: 'USDC', label: 'USDC', valor: 'usd-coin', dto: 'usdc' },
  { nombre: 'Litecoin', clave: 'Litecoin', label: 'LTC', valor: 'litecoin', dto: 'litecoin' },
  { nombre: 'Dogecoin', clave: 'Dogecoin', label: 'DOGE', valor: 'dogecoin', dto: 'dogecoin' }
]

watch(() => formularioCompra.value.moneda, async (nuevaMoneda) => {
  formularioCompra.value.cantidad = 0
  formularioCompra.value.monto = 0
  
  cargandoPrecio.value = true
  const datoPrecio = await obtenerPrecioMoneda(nuevaMoneda)
  formularioCompra.value.precioUnitario = datoPrecio.precio
  formularioCompra.value.exchange = datoPrecio.exchange
  cargandoPrecio.value = false
})

watch(() => formularioVenta.value.moneda, async (nuevaMoneda) => {
  formularioVenta.value.cantidad = 0
  formularioVenta.value.monto = 0
  
  cargandoPrecio.value = true
  const datoPrecio = await obtenerPrecioMoneda(nuevaMoneda)
  formularioVenta.value.precioUnitario = datoPrecio.precio
  formularioVenta.value.exchange = datoPrecio.exchange
  cargandoPrecio.value = false
})

function calcularValorEnPesos(cantidad, moneda) {
  // Mapeo de nombres de monedas a claves de tasas
  const monedaMap = {
    'bitcoin': 'BITCOIN',
    'ethereum': 'ETHEREUM',
    'usd-coin': 'USDC',
    'litecoin': 'LITECOIN',
    'dogecoin': 'DOGECOIN'
  }

  const claveTasa = monedaMap[moneda] || moneda.toUpperCase()
  const tasa = tasasCambio.value[claveTasa] || 0    

  if (!cantidad || !tasa) {
    return '0.00'
  } 

  return (cantidad * tasa)?.toFixed(2)
}

function calcularMontoCompra() {
  const cantidad = formularioCompra.value.cantidad
  const precio = formularioCompra.value.precioUnitario
  
  console.log('calcularMontoCompra - valores:', { cantidad, precio, tipo_cantidad: typeof cantidad, tipo_precio: typeof precio })
  
  if (cantidad > 0 && precio > 0) {
    const montoCalculado = cantidad * precio
    formularioCompra.value.monto = montoCalculado
    console.log('✓ Monto calculado correctamente:', { cantidad, precio, monto: montoCalculado })
  } else {
    formularioCompra.value.monto = 0
    console.log('✗ Monto reseteado - valores inválidos:', { cantidad, precio })
  }
}

function calcularMontoVenta() {
  const cantidad = formularioVenta.value.cantidad
  const precio = formularioVenta.value.precioUnitario
  
  console.log('calcularMontoVenta - valores:', { cantidad, precio, tipo_cantidad: typeof cantidad, tipo_precio: typeof precio })
  
  if (cantidad > 0 && precio > 0) {
    const montoCalculado = cantidad * precio
    formularioVenta.value.monto = montoCalculado
    console.log('✓ Monto calculado correctamente:', { cantidad, precio, monto: montoCalculado })
  } else {
    formularioVenta.value.monto = 0
    console.log('✗ Monto reseteado - valores inválidos:', { cantidad, precio })
  }
}

function abrirModalCompra() {
  mostrarModalCompra.value = true
}

function cerrarModalCompra() {
  mostrarModalCompra.value = false
  formularioCompra.value = {
    moneda: 'Bitcoin',
    cantidad: 0,
    monto: 0,
    precioUnitario: 0,
    exchange: 'satoshitango'
  }
  mensajeCompra.value = null
}

function abrirModalVenta() {
  mostrarModalVenta.value = true
}

function cerrarModalVenta() {
  mostrarModalVenta.value = false
  formularioVenta.value = {
    moneda: 'Bitcoin',
    cantidad: 0,
    monto: 0,
    precioUnitario: 0,
    exchange: 'satoshitango'
  }
  mensajeVenta.value = null
}

async function realizarCompra() {
  console.log('Iniciando compra con:', {
    cantidad: formularioCompra.value.cantidad,
    monto: formularioCompra.value.monto,
    precioUnitario: formularioCompra.value.precioUnitario
  })

  // Cantidad es la cantidad de criptomonedas (ej: 0.5 BTC)
  const cantidadCriptos = parseFloat(formularioCompra.value.cantidad)
  
  if (isNaN(cantidadCriptos) || cantidadCriptos <= 0) {
    console.warn('Cantidad inválida:', cantidadCriptos)
    mensajeCompra.value = { tipo: 'error', texto: 'Ingresa una cantidad válida' }
    return
  }

  // Monto es el valor en pesos a pagar (cantidad * precio unitario)
  const valorEnPesos = parseFloat(formularioCompra.value.monto)
  console.log('Valor en pesos parseado:', valorEnPesos)
  
  if (isNaN(valorEnPesos) || valorEnPesos <= 0) {
    console.warn('Valor en pesos inválido:', valorEnPesos)
    mensajeCompra.value = { tipo: 'error', texto: 'Error al calcular el monto. Verifica que hayas ingresado una cantidad.' }
    return
  }

  if (billetera.value.saldo < valorEnPesos) {
    mensajeCompra.value = { tipo: 'error', texto: `Saldo insuficiente. Necesitas $${valorEnPesos.toFixed(2)} ARS` }
    return
  }

  try {
    cargandoCompra.value = true
    const usuarioId = JSON.parse(localStorage.getItem('user')).id

    const monedaMap = {
      'bitcoin': 'BITCOIN',
      'ethereum': 'ETHEREUM',
      'usd-coin': 'USDC',
      'litecoin': 'LITECOIN',
      'dogecoin': 'DOGECOIN'
    }

    const transaccion = {
      usuarioId: usuarioId,
      operacion: 'Compra',
      moneda: monedaMap[formularioCompra.value.moneda] || formularioCompra.value.moneda.toUpperCase(),
      monto: cantidadCriptos,        // Cantidad de criptos (ej: 0.5)
      valor: valorEnPesos,            // Valor en pesos (ej: 90000)
      fecha: new Date().toISOString()
    }

    console.log('Transacción validada y lista para enviar:', transaccion)
    console.log('Se debitarán $' + valorEnPesos.toFixed(2) + ' ARS del saldo')

    const response = await crearTransaccion(transaccion)

    console.log('Respuesta:', response)

    if (response.error) {
      mensajeCompra.value = { tipo: 'error', texto: response.error }
    } else if (response.success) {
      mensajeCompra.value = { 
        tipo: 'exito', 
        texto: 'Compra realizada exitosamente' 
      }
      setTimeout(() => {
        location.reload()
      }, 1500)
    }
  } catch (err) {
    console.error('Error:', err)
    mensajeCompra.value = { tipo: 'error', texto: err.message || 'Error al realizar la compra' }
  } finally {
    cargandoCompra.value = false
  }
}

async function realizarVenta() {
  console.log('Iniciando venta con:', {
    cantidad: formularioVenta.value.cantidad,
    monto: formularioVenta.value.monto,
    precioUnitario: formularioVenta.value.precioUnitario
  })

  // Cantidad es la cantidad de criptomonedas a vender (ej: 0.5 BTC)
  const cantidadCriptos = parseFloat(formularioVenta.value.cantidad)
  
  if (isNaN(cantidadCriptos) || cantidadCriptos <= 0) {
    console.warn('Cantidad inválida:', cantidadCriptos)
    mensajeVenta.value = { tipo: 'error', texto: 'Ingresa una cantidad válida' }
    return
  }

  // Monto es el valor en pesos que recibirás (cantidad * precio unitario)
  const valorEnPesos = parseFloat(formularioVenta.value.monto)
  console.log('Valor en pesos parseado:', valorEnPesos)
  
  if (isNaN(valorEnPesos) || valorEnPesos <= 0) {
    console.warn('Valor en pesos inválido:', valorEnPesos)
    mensajeVenta.value = { tipo: 'error', texto: 'Error al calcular el monto. Verifica que hayas ingresado una cantidad.' }
    return
  }

  // Mapeo de monedas para obtener el saldo correcto de billetera
  const monedaMapBilletera = {
    'Bitcoin': 'bitCoin',
    'Ethereum': 'ethereum',
    'USDC': 'usdc',
    'Litecoin': 'litecoin',
    'Dogecoin': 'dogecoin'
  }

  const claveBilletera = monedaMapBilletera[formularioVenta.value.moneda]
  const saldoMoneda = billetera.value[claveBilletera]
console.log('Saldo de la moneda a vender:', saldoMoneda, 'Cantidad a vender:', cantidadCriptos)

  if (saldoMoneda < cantidadCriptos) {
    mensajeVenta.value = { tipo: 'error', texto: `No tienes suficiente ${formularioVenta.value.moneda.toUpperCase()}. Tienes: ${saldoMoneda.toFixed(8)}` }
    return
  }

  try {
    cargandoVenta.value = true
    const usuarioId = JSON.parse(localStorage.getItem('user')).id

    const transaccion = {
      usuarioId: usuarioId,
      operacion: 'Venta',
      moneda: monedaMapBilletera[formularioVenta.value.moneda] || formularioVenta.value.moneda.toUpperCase(),
      monto: cantidadCriptos,        // Cantidad de criptos a vender (ej: 0.5)
      valor: valorEnPesos,            // Valor en pesos que recibirás (ej: 90000)
      fecha: new Date().toISOString()
    }

    console.log('Transacción validada y lista para enviar:', transaccion)
    console.log('Recibirás $' + valorEnPesos.toFixed(2) + ' ARS en tu saldo')

    const response = await crearTransaccion(transaccion)

    console.log('Respuesta:', response)

    if (response.error) {
      mensajeVenta.value = { tipo: 'error', texto: response.error }
    } else if (response.success) {
      mensajeVenta.value = { 
        tipo: 'exito', 
        texto: 'Venta realizada exitosamente' 
      }
      setTimeout(() => {
        location.reload()
      }, 1500)
    }
  } catch (err) {
    console.error('Error:', err)
    mensajeVenta.value = { tipo: 'error', texto: err.message || 'Error al realizar la venta' }
  } finally {
    cargandoVenta.value = false
  }
}

function verTransacciones() {
  router.push('/Transacciones')
}

onMounted(async () => {
  try {
    const usuarioId = JSON.parse(localStorage.getItem('user')).id
    if (!usuarioId) {
      error.value = 'Usuario no identificado'
      return
    }

    usuario.value = JSON.parse(localStorage.getItem('user'))
    billetera.value = await obtenerBilletera(usuarioId)
    console.log('Billetera cargada:', billetera.value)
    tasasCambio.value = await obtenerTasasCambio()
    
    console.log('Tasas de cambio cargadas:', tasasCambio.value)

    const datoPrecio = await obtenerPrecioMoneda('bitcoin')
    formularioCompra.value.precioUnitario = datoPrecio.precio
    formularioCompra.value.exchange = datoPrecio.exchange

    formularioVenta.value.precioUnitario = datoPrecio.precio
    formularioVenta.value.exchange = datoPrecio.exchange
    
  } catch (err) {
    console.error(err)
    error.value = 'Error al cargar los datos'
  } finally {
    cargando.value = false
  }
})
</script>

<template>
  <div class="contenedor">
    <div v-if="cargando" class="cargando">
      <p>Cargando datos...</p>
    </div>

    <div v-else-if="error" class="error">
      <p>{{ error }}</p>
    </div>

    <div v-else>
      <!-- Datos del Usuario -->
      <div class="perfil-usuario">
        <h1 class="title">Whalet</h1>
        <div class="usuario-info">
          <div v-if="usuario.foto" class="foto-usuario">
            <img :src="usuario.foto" :alt="usuario.nombre">
          </div>
          <div class="datos-usuario">
            <h2>{{ usuario.nombre }} {{ usuario.apellido }}</h2>
            <p class="email">{{ usuario.email }}</p>
          </div>
        </div>
      </div>

      <!-- Saldo Principal en Pesos -->
      <div class="saldo-principal">
        <p class="label">Saldo Total</p>
        <h3 class="cantidad">${{ billetera.saldo?.toFixed(2) || '0.00' }} ARS</h3>
      </div>

      <!-- Lista de Criptomonedas -->
  <div class="monedas-container">   
    <h3>Tus Criptomonedas</h3>
    <div class="lista-monedas">
      <div v-for="moneda in monedas" :key="moneda.clave" class="moneda-card">
        <div class="moneda-header">
          <h4>{{ moneda.nombre }}</h4>
          <span class="cantidad-cripto">
            {{ billetera[moneda.dto]?.toFixed(8) || '0.00000000' }}
          </span>
        </div>
        <div class="moneda-valor">
          <p class="valor-pesos">
            ${{ calcularValorEnPesos(billetera[moneda.dto]?.toFixed(8) || 0, moneda.dto) }} ARS
          </p>
        </div>
      </div>
    </div>
  </div>

      <button @click="abrirModalCompra" class="btn-transacciones">
        Comprar
      </button>
    <br>
    <br>
    <button @click="abrirModalVenta" class="btn-transacciones btn-vender">
          Vender
        </button>
      </div>
    <br>
      <button @click="verTransacciones" class="btn-transacciones">
        Ver Transacciones
      </button>
    </div>

  <div v-if="mostrarModalCompra" class="modal-overlay" @click.self="cerrarModalCompra">
      <div class="modal">
        <div class="modal-header">
          <h2>Comprar Criptomonedas</h2>
          <button class="btn-cerrar" @click="cerrarModalCompra">&times;</button>
        </div>

        <div class="modal-body">
          <!-- Mensaje de estado -->
          <div v-if="mensajeCompra" :class="['mensaje', mensajeCompra.tipo]">
            {{ mensajeCompra.texto }}
          </div>

          <!-- Selector de Moneda -->

            <div class="formulario-grupo">
            <label>Selecciona la Criptomoneda:</label>
            <select v-model="formularioCompra.moneda" :disabled="cargandoPrecio">
                <option v-for="moneda in monedas" :key="moneda.clave" :value="moneda.valor">
                {{ moneda.nombre }}
                </option>
            </select>
            </div>

          <div class="formulario-grupo">
            <label>Exchange:</label>
            <input 
              type="text" 
              :value="formularioCompra.exchange" 
              disabled 
              style="background-color: #f5f5f5; cursor: not-allowed;"
            >
          </div>

          <!-- Cantidad a Comprar -->
          <div class="formulario-grupo">
            <label>Cantidad:</label>
            <input
              :value="formularioCompra.cantidad"
              type="number"
              placeholder="0.00"
              step="0.00000001"
              @input="(e) => { formularioCompra.cantidad = parseFloat(e.target.value) || 0; calcularMontoCompra() }"
            >
          </div>

          <!-- Tasa de Cambio -->
          <div class="info-tasa">
            <p>Precio Unitario: ${{ formularioCompra.precioUnitario.toLocaleString('es-AR', { maximumFractionDigits: 2 }) }} ARS</p>
          </div>

          <!-- Monto Total -->
          <div class="monto-total">
            <p class="label">Total a Pagar:</p>
            <p class="cantidad">${{ parseFloat(formularioCompra.monto).toLocaleString('es-AR') }} ARS</p>
          </div>

          <!-- Saldo Disponible -->
          <div class="saldo-disponible">
            <p>Saldo Disponible: ${{ billetera.saldo?.toFixed(2) || '0.00' }} ARS</p>
          </div>

        <div class="modal-footer">
          <button @click="cerrarModalCompra" class="btn-cancelar">
            Cancelar
          </button>
          <button 
            @click="realizarCompra" 
            class="btn-comprar"
            :disabled="cargandoCompra"
          >
            {{ cargandoCompra ? 'Procesando...' : 'Comprar' }}
          </button>
        </div>
      </div>
  </div>/div>
    </div>
    <!-- Modal Venta -->
    <div v-if="mostrarModalVenta" class="modal-overlay" @click.self="cerrarModalVenta">
      <div class="modal">
        <div class="modal-header">
          <h2>Vender Criptomonedas</h2>
          <button class="btn-cerrar" @click="cerrarModalVenta">&times;</button>
        </div>

        <div class="modal-body">
          <div v-if="mensajeVenta" :class="['mensaje', mensajeVenta.tipo]">
            {{ mensajeVenta.texto }}
          </div>

        <div class="formulario-grupo">
         <label>Selecciona la Criptomoneda:</label>
          <select v-model="formularioVenta.moneda" :disabled="cargandoPrecio">
            <option v-for="moneda in monedas" :key="moneda.clave" :value="moneda.valor">
            {{ moneda.nombre }}
            </option>
          </select>
        </div>

          <div class="formulario-grupo">
            <label>Exchange:</label>
            <input 
              type="text" 
              :value="formularioVenta.exchange" 
              disabled 
              style="background-color: #f5f5f5; cursor: not-allowed;"
            >
          </div>

          <div class="formulario-grupo">
            <label>Cantidad:</label>
            <input
              :value="formularioVenta.cantidad"
              type="number"
              placeholder="0.00"
              step="0.00000001"
              @input="(e) => { formularioVenta.cantidad = parseFloat(e.target.value) || 0; calcularMontoVenta() }"
            >
          </div>

          <div class="info-tasa">
            <p>Precio Unitario: ${{ formularioVenta.precioUnitario.toLocaleString('es-AR', { maximumFractionDigits: 2 }) }} ARS</p>
          </div>

          <div class="monto-total">
            <p class="label">Total a Recibir:</p>
            <p class="cantidad">${{ parseFloat(formularioVenta.monto).toLocaleString('es-AR') }} ARS</p>
          </div>

          <div class="saldo-disponible">
            <p>
                Saldo Disponible: 
                {{ 
                billetera[
                    formularioVenta.moneda === 'bitcoin' ? 'bitCoin' :
                    formularioVenta.moneda === 'ethereum' ? 'ethereum' :
                    formularioVenta.moneda === 'usd-coin' ? 'usdc' :
                    formularioVenta.moneda === 'litecoin' ? 'litecoin' :
                    formularioVenta.moneda === 'dogecoin' ? 'dogecoin' : ''
                ]?.toFixed(8) || '0.00000000' 
                }} 
                {{ formularioVenta.moneda.toUpperCase() }}
            </p>
</div>
        </div>

        <div class="modal-footer">
          <button @click="cerrarModalVenta" class="btn-cancelar">
            Cancelar
          </button>
          <button 
            @click="realizarVenta" 
            class="btn-vender"
            :disabled="cargandoVenta"
          >
            {{ cargandoVenta ? 'Procesando...' : 'Vender' }}
          </button>
        </div>
      </div>
    </div>
</template>

<style scoped>
.contenedor {
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
}

.title {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-size: 3rem;
  font-weight: bold;
  color: rgb(0, 132, 255);
  margin-bottom: 20px;
}

.perfil-usuario {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 10px;
  margin-bottom: 30px;
}

.usuario-info {
  display: flex;
  align-items: center;
  gap: 20px;
}

.foto-usuario {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  overflow: hidden;
  background: #ddd;
}

.foto-usuario img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.datos-usuario h2 {
  margin: 0;
  color: #333;
}

.email {
  margin: 5px 0 0 0;
  color: #666;
}

.saldo-principal {
  background: linear-gradient(135deg, rgb(0, 132, 255), rgb(0, 100, 200));
  color: white;
  padding: 30px;
  border-radius: 15px;
  text-align: center;
  margin-bottom: 30px;
}

.label {
  margin: 0;
  opacity: 0.9;
}

.cantidad {
  margin: 10px 0 0 0;
  font-size: 2.5rem;
}

.monedas-container h3 {
  color: rgb(0, 132, 255);
  margin-bottom: 15px;
}

.lista-monedas {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 15px;
  margin-bottom: 30px;
}

.moneda-card {
  background: white;
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  padding: 20px;
  transition: all 0.3s ease;
}

.moneda-card:hover {
  border-color: rgb(0, 132, 255);
  box-shadow: 0 4px 12px rgba(0, 132, 255, 0.2);
}

.moneda-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.moneda-header h4 {
  margin: 0;
  color: #333;
}

.cantidad-cripto {
  font-weight: bold;
  color: rgb(0, 132, 255);
  font-size: 0.9rem;
}

.moneda-valor {
  border-top: 1px solid #eee;
  padding-top: 10px;
}

.valor-pesos {
  margin: 0;
  color: #666;
  font-size: 0.95rem;
}

.botones-container {
  display: flex;
  gap: 10px;
}

.btn-transacciones {
  flex: 1;
  padding: 12px;
  background-color: rgb(0, 132, 255);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.btn-transacciones:hover {
  background-color: rgb(0, 100, 200);
}

.btn-vender {
  background-color: rgb(244, 67, 54);
}

.btn-vender:hover {
  background-color: rgb(211, 47, 47);
}

.cargando, .error {
  text-align: center;
  padding: 40px;
  font-size: 1.1rem;
}

.error {
  color: red;
}

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

.info-tasa {
  background: #f0f8ff;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 15px;
}

.info-tasa p {
  margin: 0;
  color: #666;
  font-size: 0.9rem;
}

.monto-total {
  background: linear-gradient(135deg, rgb(0, 132, 255), rgb(0, 100, 200));
  color: white;
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 15px;
  text-align: center;
}

.monto-total .label {
  margin: 0;
  opacity: 0.9;
  font-size: 0.9rem;
}

.monto-total .cantidad {
  margin: 8px 0 0 0;
  font-size: 1.8rem;
}

.saldo-disponible {
  background: #f0f0f0;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 15px;
}

.saldo-disponible p {
  margin: 0;
  color: #666;
  font-size: 0.9rem;
}

.mensaje {
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 15px;
  font-weight: bold;
  text-align: center;
  font-size: 0.95rem;
}

.mensaje.error {
  background: #ffebee;
  color: #c62828;
  border: 1px solid #ef5350;
}

.mensaje.exito {
  background: #e8f5e9;
  color: #2e7d32;
  border: 1px solid #66bb6a;
}

.modal-footer {
  display: flex;
  gap: 10px;
  padding: 20px;
  border-top: 1px solid #eee;
}

.btn-cancelar,
.btn-comprar,
.btn-vender {
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

.btn-comprar {
  background: rgb(0, 132, 255);
  color: white;
}

.btn-comprar:hover:not(:disabled) {
  background: rgb(0, 100, 200);
}

.btn-vender {
  background: rgb(244, 67, 54);
  color: white;
}

.btn-vender:hover:not(:disabled) {
  background: rgb(211, 47, 47);
}

.btn-comprar:disabled,
.btn-vender:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>