export const obtenerTasasCambio = async () => {
  try {
    // Obtener tasas de múltiples monedas de CriptoYa
    const monedas = {
      BTC: 'BITCOIN',
      ETH: 'ETHEREUM',
      USDC: 'USDC',
      LTC: 'LITECOIN',
      DOGE: 'DOGECOIN'
    }

    const tasas = {}

    for (const [simbolo, nombre] of Object.entries(monedas)) {
      try {
        const response = await fetch(
          `https://criptoya.com/api/${simbolo}/ARS/1`
        )
        
        if (!response.ok) throw new Error(`Error al obtener tasa de ${simbolo}`)
        
        const data = await response.json()
        
        // Obtener precio de satoshitango específicamente
        // satoshitango es un objeto con bid/ask, así que extraemos el bid
        const satoshitangoData = data.satoshitango
        let precioSatoshitango = 0
        
        if (typeof satoshitangoData === 'object' && satoshitangoData.bid) {
          precioSatoshitango = satoshitangoData.bid
        } else if (typeof satoshitangoData === 'number') {
          precioSatoshitango = satoshitangoData
        }
        
        console.log(`Tasa de ${nombre} (satoshitango):`, precioSatoshitango)
        
        tasas[nombre] = precioSatoshitango
      } catch (error) {
        console.error(`Error obteniendo tasa de ${simbolo}:`, error)
        tasas[nombre] = 0
      }
    }

    return tasas
  } catch (error) {
    console.error('Error obteniendo tasas:', error)
    // Retorna tasas por defecto si falla
    return {
      BITCOIN: 2800000,
      ETHEREUM: 180000,
      USDC: 1000,
      LITECOIN: 12000,
      DOGECOIN: 15
    }
  }
}

export const obtenerPrecioMoneda = async (moneda) => {
  try {
    // Mapear nombres de monedas
    const monedaMap = {
      'bitcoin': 'BTC',
      'ethereum': 'ETH',
      'usd-coin': 'USDC',
      'litecoin': 'LTC',
      'dogecoin': 'DOGE'
    }

    const simboloMoneda = monedaMap[moneda.toLowerCase()] || moneda.toUpperCase()

    const response = await fetch(
      `https://criptoya.com/api/${simboloMoneda}/ARS/1`
    )

    if (!response.ok) throw new Error(`Error al obtener precio de ${simboloMoneda}`)

    const data = await response.json()

    // Obtener precio de satoshitango específicamente
    // satoshitango es un objeto con bid/ask, así que extraemos el bid
    const satoshitangoData = data.satoshitango
    let precio = 0
    
    if (typeof satoshitangoData === 'object' && satoshitangoData.bid) {
      precio = satoshitangoData.bid
    } else if (typeof satoshitangoData === 'number') {
      precio = satoshitangoData
    }

    return {
      precio: precio,
      exchange: 'satoshitango',
      bid: precio,
      ask: satoshitangoData?.ask || precio
    }
  } catch (error) {
    console.error(`Error obteniendo precio de ${moneda}:`, error)
    return {
      precio: 0,
      exchange: 'satoshitango',
      bid: 0,
      ask: 0
    }
  }
}

export const obtenerExchanges = async (moneda = null) => {
  try {
    // Mapear nombres de monedas
    const monedaMap = {
      'bitcoin': 'BTC',
      'ethereum': 'ETH',
      'usd-coin': 'USDC',
      'litecoin': 'LTC',
      'dogecoin': 'DOGE'
    }

    const simboloMoneda = monedaMap[moneda?.toLowerCase()] || moneda?.toUpperCase() || 'BTC'

    const response = await fetch(
      `https://criptoya.com/api/${simboloMoneda}/ARS/1`
    )

    if (!response.ok) throw new Error(`Error al obtener exchanges para ${simboloMoneda}`)

    const data = await response.json()

    // Convertir el objeto de exchanges en un array
    const exchanges = Object.entries(data)
      .filter(([key, value]) => typeof value === 'number' && value > 0)
      .map(([key, precio]) => ({
        id: key,
        nombre: key.charAt(0).toUpperCase() + key.slice(1),
        precio: precio,
        moneda: simboloMoneda
      }))
      .sort((a, b) => a.precio - b.precio) // Ordenar por precio ascendente

    return exchanges.length > 0 ? exchanges : []
  } catch (error) {
    console.error(`Error en obtenerExchanges:`, error)
    return []
  }
}

// Función específica para obtener precio de satoshitango
export const obtenerPrecioSatoshitango = async (moneda, volumen = 1) => {
  try {
    // Mapear nombres de monedas
    const monedaMap = {
      'bitcoin': 'BTC',
      'ethereum': 'ETH',
      'usd-coin': 'USDC',
      'litecoin': 'LTC',
      'dogecoin': 'DOGE'
    }

    const simboloMoneda = monedaMap[moneda?.toLowerCase()] || moneda?.toUpperCase()

    const response = await fetch(
      `https://criptoya.com/api/satoshitango/${simboloMoneda}/ARS/${volumen}`
    )

    if (!response.ok) throw new Error(`Error al obtener precio de satoshitango`)

    const data = await response.json()

    return {
      precio: data.precio || 0,
      bid: data.bid || 0,
      ask: data.ask || 0,
      exchange: 'satoshitango',
      moneda: simboloMoneda,
      volumen: volumen
    }
  } catch (error) {
    console.error(`Error en obtenerPrecioSatoshitango:`, error)
    return {
      precio: 0,
      bid: 0,
      ask: 0,
      exchange: 'satoshitango',
      moneda: moneda,
      volumen: volumen
    }
  }
}