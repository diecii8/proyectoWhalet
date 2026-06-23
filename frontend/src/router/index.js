import { createRouter, createWebHistory } from 'vue-router'

import InicioSesion from '../components/InicioSesion.vue'
import BilleteraUsuario from '../components/BilleteraUsuario.vue'
import Transacciones from '../components/Transacciones.vue'

const routes = [
  {
    path: '/',
    component: InicioSesion
  },
  {
    path: '/BilleteraUsuario',
    component: BilleteraUsuario
  },
  {
    path: '/Transacciones',
    component: Transacciones
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router