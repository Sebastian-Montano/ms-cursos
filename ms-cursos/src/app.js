// =====================================================================
// ms-cursos · microservicio de cursos relacionado con ms-programas
// (código de referencia de la ficha 3).
// =====================================================================
const express = require('express');
const cors = require('cors');
const rutas = require('./routes/cursos.routes');

const app = express();

app.use(cors());
app.use(express.json());

// Registro: muestra en la terminal cada petición que llega
app.use((req, res, next) => {
  console.log(`${new Date().toLocaleTimeString()} ${req.method} ${req.originalUrl}`);
  next();
});

app.get('/health', (req, res) => {
  res.json({ servicio: 'ms-cursos', estado: 'ok' });
});

app.use('/cursos', rutas);

app.use((req, res) => {
  res.status(404).json({ error: `Ruta no encontrada: ${req.method} ${req.originalUrl}` });
});

// Manejador de errores: convierte HttpError (y JSON mal formado) en respuesta JSON
app.use((err, req, res, next) => {
  if (err.type === 'entity.parse.failed') {
    return res.status(400).json({ error: 'El cuerpo no es un JSON válido' });
  }
  const status = err.status || 500;
  if (status === 500) console.error(err);
  const cuerpo = { error: status === 500 ? 'Error interno del servidor' : err.message };
  if (err.detalles) cuerpo.detalles = err.detalles;
  res.status(status).json(cuerpo);
});

module.exports = app;
