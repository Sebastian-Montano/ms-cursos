// CAPA DE NEGOCIO: cursos
const repo = require('../repositories/cursos.repository');
const { validar } = require('../models/curso.model');
const programas = require('../clients/programas.client');
const HttpError = require('../utils/http-error');

// Agrega a cada curso su objeto programa (composición).
// Si ms-programas no responde: programa null y un aviso, pero igual 200.
async function componer(cursos, token) {
  const mapa = await programas.obtenerProgramas(cursos.map((c) => c.programa_id), token);
  return cursos.map((c) =>
    mapa
      ? { ...c, programa: mapa.get(c.programa_id) || null }
      : { ...c, programa: null, aviso: programas.AVISO }
  );
}

// Ficha 3: valida la relación y aplica la regla de negocio
async function validarRelacion(curso, token) {
  const programa = await programas.obtenerPrograma(curso.programa_id, token); // 503 · 400
  if (curso.creditos > programa.creditos_max_curso) {
    throw new HttpError(
      400,
      `Los créditos del curso (${curso.creditos}) superan el máximo del programa ${programa.nombre_programa} (${programa.creditos_max_curso})`
    );
  }
  return programas.resumen(programa);
}

async function listar(filtros, token) {
  return componer(await repo.findAll(filtros), token);
}

async function obtener(id, token) {
  const curso = await repo.findById(id);
  if (!curso) throw new HttpError(404, `No existe el curso con id ${id}`);
  const [compuesto] = await componer([curso], token);
  return compuesto;
}

async function crear(datos, token) {
  const curso = validar(datos);                              // 400 datos inválidos
  const programa = await validarRelacion(curso, token);      // 503 · 400 no existe · 400 regla
  if (await repo.findByCodigo(curso.codigo)) {
    throw new HttpError(409, `Ya existe un curso con el código ${curso.codigo}`);
  }
  const creado = await repo.create(curso);
  return { ...creado, programa };
}

async function actualizar(id, datos, token) {
  if (!(await repo.findById(id))) throw new HttpError(404, `No existe el curso con id ${id}`);
  const curso = validar(datos);
  const programa = await validarRelacion(curso, token);
  const otro = await repo.findByCodigo(curso.codigo);
  if (otro && otro.id !== id) {
    throw new HttpError(409, `Ya existe un curso con el código ${curso.codigo}`);
  }
  const actualizado = await repo.update(id, curso);
  return { ...actualizado, programa };
}

async function eliminar(id) {
  if (!(await repo.remove(id))) throw new HttpError(404, `No existe el curso con id ${id}`);
}

module.exports = { listar, obtener, crear, actualizar, eliminar };
