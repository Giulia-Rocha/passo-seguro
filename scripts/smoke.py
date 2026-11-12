"""Exercise the running Docker API and confirm persistence after restart; never log secrets."""
import json
import pathlib
import subprocess
import time
import urllib.request
import uuid

root = pathlib.Path(__file__).resolve().parent.parent
env = dict(line.split('=', 1) for line in (root / '.env').read_text().splitlines() if '=' in line)
base = 'http://localhost:5080/api/v1'


def call(path, method='GET', data=None, token=None, key=None):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    if key:
        headers['Idempotency-Key'] = key
    request = urllib.request.Request(base + path, data=None if data is None else json.dumps(data).encode(), headers=headers, method=method)
    with urllib.request.urlopen(request, timeout=20) as response:
        body = response.read()
        return response.status, json.loads(body) if body else None


health = call('/health')
_, auth = call('/auth/login', 'POST', {'email': 'cidadao@passoseguro.test', 'password': env['DEMO_PASSWORD']})
token = auth['accessToken']
payload = {'categoryId': 1, 'description': 'Ocorrência fictícia de verificação de persistência.', 'difficulty': 3, 'latitude': -23.557, 'longitude': -46.646}
key = str(uuid.uuid4())
status, created = call('/occurrences', 'POST', payload, token, key)
occurrence_id = created['id']
_, replay = call('/occurrences', 'POST', payload, token, key)
assert replay['id'] == occurrence_id
payload['difficulty'] = 4
call('/occurrences/' + occurrence_id, 'PUT', payload, token)
subprocess.run(['docker', 'compose', 'restart', 'db', 'api'], cwd=root, check=True, capture_output=True)
for attempt in range(60):
    try:
        call('/health')
        break
    except Exception:
        if attempt == 59:
            raise
        time.sleep(1)
_, restored = call('/occurrences/' + occurrence_id, token=token)
assert restored['difficulty'] == 4
_, indicators = call('/indicators', token=token)
delete_status, _ = call('/occurrences/' + occurrence_id, 'DELETE', token=token)
call('/auth/logout', 'POST', {'refreshToken': auth['refreshToken']})
result = {'health': health[0], 'create': status, 'idempotency': 'same identifier', 'persistence_after_database_and_api_restart': True, 'updated_difficulty': restored['difficulty'], 'indicators_total_during_test': indicators['total'], 'delete': delete_status, 'data': 'fictitious', 'timestamp_utc': time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime())}
target = root / 'docs' / 'evidencias'
target.mkdir(exist_ok=True)
(target / 'api-smoke.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
print(json.dumps(result))
