// CAPA DE ACCESO A DATOS: cursos en MySQL
const pool = require('../db');

async function findAll({ semestre } = {}) {
  const [filas] = semestre
    ? await pool.query('SELECT * FROM cursos WHERE semestre = ? ORDER BY id', [semestre])
    : await pool.query('SELECT * FROM cursos ORDER BY id');
  return filas;
}

async function findById(id) {
  const [filas] = await pool.query('SELECT * FROM cursos WHERE id = ?', [id]);
  return filas[0] || null;
}

async function findByCodigo(codigo) {
  const [filas] = await pool.query('SELECT * FROM cursos WHERE codigo = ?', [codigo]);
  return filas[0] || null;
}

async function create(c) {
  const [r] = await pool.query(
    'INSERT INTO cursos (codigo, nombre, creditos, cupo, semestre, programa_id) VALUES (?, ?, ?, ?, ?, ?)',
    [c.codigo, c.nombre, c.creditos, c.cupo, c.semestre, c.programa_id]
  );
  return findById(r.insertId);
}

async function update(id, c) {
  await pool.query(
    'UPDATE cursos SET codigo = ?, nombre = ?, creditos = ?, cupo = ?, semestre = ?, programa_id = ? WHERE id = ?',
    [c.codigo, c.nombre, c.creditos, c.cupo, c.semestre, c.programa_id, id]
  );
  return findById(id);
}

async function remove(id) {
  const [r] = await pool.query('DELETE FROM cursos WHERE id = ?', [id]);
  return r.affectedRows > 0;
}

module.exports = { findAll, findById, findByCodigo, create, update, remove };
