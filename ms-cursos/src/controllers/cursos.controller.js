// CAPA DE PRESENTACIÓN: HTTP ↔ servicio de cursos
const service = require('../services/cursos.service');
const HttpError = require('../utils/http-error');

function leerId(req) {
  const id = Number(req.params.id);
  if (!Number.isInteger(id) || id <= 0) throw new HttpError(400, 'El id debe ser un entero positivo');
  return id;
}

// GET /cursos  ·  GET /cursos?semestre=2026-2
async function listar(req, res) {
  const { semestre } = req.query;
  res.status(200).json(await service.listar({ semestre }, req.headers.authorization));
}

// GET /cursos/:id
async function obtener(req, res) {
  res.status(200).json(await service.obtener(leerId(req), req.headers.authorization));
}

// POST /cursos
async function crear(req, res) {
  const curso = await service.crear(req.body, req.headers.authorization);
  res.location(`/cursos/${curso.id}`).status(201).json(curso);
}

// PUT /cursos/:id
async function actualizar(req, res) {
  res.status(200).json(await service.actualizar(leerId(req), req.body, req.headers.authorization));
}

// DELETE /cursos/:id
async function eliminar(req, res) {
  await service.eliminar(leerId(req));
  res.status(204).end();
}

module.exports = { listar, obtener, crear, actualizar, eliminar };
