// CAPA DE PRESENTACIÓN: rutas de cursos
const { Router } = require('express');
const controller = require('../controllers/cursos.controller');
const { verificarToken, requiereRol } = require('../middlewares/auth');

const router = Router();

router.use(verificarToken);                                          // todas las rutas exigen token

router.get('/', controller.listar);                                  // GET /cursos  y  GET /cursos?semestre=2026-2
router.get('/:id', controller.obtener);                              // GET /cursos/:id
router.post('/', requiereRol('administrador'), controller.crear);        // POST /cursos
router.put('/:id', requiereRol('administrador'), controller.actualizar); // PUT /cursos/:id
router.delete('/:id', requiereRol('administrador'), controller.eliminar);// DELETE /cursos/:id

module.exports = router;
