// =====================================================================
// CLIENTE HTTP de ms-programas (ficha 3), completo.
// pedir() viene del .zip del proveedor; las otras funciones son las que
// usan la validación, la regla de negocio y la composición.
// En el .env:  PROGRAMAS_URL=http://127.0.0.1:3012
// =====================================================================
const HttpError = require('../utils/http-error');
const PROGRAMAS_URL = process.env.PROGRAMAS_URL;

const AVISO = 'ms-programas no respondió: el programa no se pudo consultar';

// Hace una petición GET a ms-programas reenviando el token del usuario.
// Devuelve la respuesta de fetch, o null si ms-programas no respondió.
async function pedir(ruta, token) {
  try {
    return await fetch(`${PROGRAMAS_URL}${ruta}`, {
      headers: { Authorization: token },     // el token de la petición original
      signal: AbortSignal.timeout(3000),      // no esperar más de 3 segundos
    });
  } catch (error) {
    return null;                              // ms-programas caído o sin responder
  }
}

// VALIDACIÓN (crear y actualizar): devuelve el programa completo,
// que la regla de negocio necesita (creditos_max_curso).
//   no responde → 503 · no existe → 400
async function obtenerPrograma(programaId, token) {
  const respuesta = await pedir(`/programas/${programaId}`, token);
  if (!respuesta) throw new HttpError(503, 'ms-programas no está disponible; intente más tarde');
  if (respuesta.status === 404) throw new HttpError(400, `No existe el programa con id ${programaId}`);
  if (!respuesta.ok) throw new HttpError(502, `ms-programas respondió ${respuesta.status}`);
  return respuesta.json();
}

// COMPOSICIÓN: el objeto que va dentro de cada curso
const resumen = (p) => ({ id: p.id, nombre_programa: p.nombre_programa, facultad: p.facultad });

// Trae varios programas en UNA sola petición (?ids=1,2,3).
// Devuelve un Map id → programa, o null si ms-programas no respondió.
async function obtenerProgramas(ids, token) {
  const unicos = [...new Set(ids)];
  if (unicos.length === 0) return new Map();
  const respuesta = await pedir(`/programas?ids=${unicos.join(',')}`, token);
  if (!respuesta || !respuesta.ok) return null;
  const lista = await respuesta.json();
  return new Map(lista.map((p) => [p.id, resumen(p)]));
}

module.exports = { pedir, obtenerPrograma, obtenerProgramas, resumen, AVISO };
