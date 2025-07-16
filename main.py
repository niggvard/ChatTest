from flask import Flask, request, jsonify
import sqlite3
from datetime import datetime, timedelta

app = Flask(__name__)
DB = 'chat.db'

# Ініціалізація таблиці
def init_db():
    with sqlite3.connect(DB) as conn:
        conn.execute('''
            CREATE TABLE IF NOT EXISTS messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                text TEXT NOT NULL,
                timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            )
        ''')

@app.route('/send', methods=['POST'])
def send_message():
    data = request.get_json()
    msg = data.get('text')

    if not msg:
        return {'error': 'no text'}, 400

    with sqlite3.connect(DB) as conn:
        conn.execute('INSERT INTO messages (text) VALUES (?)', (msg,))
        conn.commit()

    return {'status': 'ok'}, 201

@app.route('/messages', methods=['GET'])
def get_messages():
    try:
        days = int(request.args.get('days', 1))
    except ValueError:
        return {'error': 'invalid days'}, 400

    since = datetime.utcnow() - timedelta(days=days)

    with sqlite3.connect(DB) as conn:
        conn.row_factory = sqlite3.Row
        cursor = conn.execute(
            'SELECT text, timestamp FROM messages WHERE timestamp >= ? ORDER BY timestamp ASC',
            (since.isoformat(),)
        )
        messages = [dict(row) for row in cursor.fetchall()]

    return jsonify(messages)

if __name__ == '__main__':
    init_db()
    app.run(port=5000)