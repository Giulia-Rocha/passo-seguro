import * as SQLite from "expo-sqlite";
import { Draft } from "./types";
let connection: ReturnType<typeof SQLite.openDatabaseAsync> | undefined;
async function database() {
  if (!connection)
    connection = (async () => {
      const db = await SQLite.openDatabaseAsync("passoseguro.db");
      await db.execAsync(
        "PRAGMA journal_mode = WAL; CREATE TABLE IF NOT EXISTS cache (owner TEXT NOT NULL, key TEXT NOT NULL, value TEXT NOT NULL, updated TEXT NOT NULL, PRIMARY KEY(owner,key)); CREATE TABLE IF NOT EXISTS drafts (owner TEXT NOT NULL, key TEXT NOT NULL, value TEXT NOT NULL, PRIMARY KEY(owner,key)); PRAGMA user_version = 1;",
      );
      return db;
    })();
  return connection;
}
export const storage = {
  async cache<T>(owner: string, key: string, value: T) {
    const db = await database();
    await db.runAsync(
      "INSERT OR REPLACE INTO cache VALUES (?,?,?,?)",
      owner,
      key,
      JSON.stringify(value),
      new Date().toISOString(),
    );
  },
  async read<T>(
    owner: string,
    key: string,
  ): Promise<{ value: T; updated: string } | null> {
    const db = await database();
    const row = await db.getFirstAsync<{ value: string; updated: string }>(
      "SELECT value,updated FROM cache WHERE owner=? AND key=?",
      owner,
      key,
    );
    return row ? { value: JSON.parse(row.value), updated: row.updated } : null;
  },
  async saveDraft(owner: string, draft: Draft) {
    const db = await database();
    await db.runAsync(
      "INSERT OR REPLACE INTO drafts VALUES (?,?,?)",
      owner,
      draft.key,
      JSON.stringify(draft),
    );
  },
  async drafts(owner: string): Promise<Draft[]> {
    const db = await database();
    const rows = await db.getAllAsync<{ value: string }>(
      "SELECT value FROM drafts WHERE owner=? ORDER BY rowid DESC",
      owner,
    );
    return rows.map((row) => JSON.parse(row.value));
  },
  async deleteDraft(owner: string, key: string) {
    const db = await database();
    await db.runAsync("DELETE FROM drafts WHERE owner=? AND key=?", owner, key);
  },
  async clear(owner: string) {
    const db = await database();
    await db.withTransactionAsync(async () => {
      await db.runAsync("DELETE FROM cache WHERE owner=?", owner);
      await db.runAsync("DELETE FROM drafts WHERE owner=?", owner);
    });
  },
};
