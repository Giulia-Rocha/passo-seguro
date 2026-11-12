"""Render a report from actual TRX and Jest output, without invented test results."""
import html
import json
import pathlib
import shutil
import xml.etree.ElementTree as ET

root = pathlib.Path(__file__).resolve().parent.parent
target = root / 'docs' / 'evidencias'
target.mkdir(exist_ok=True)
ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
sections = []
for project in ['PassoSeguro.UnitTests', 'PassoSeguro.IntegrationTests']:
    source = root / 'backend' / 'tests' / project / 'TestResults' / 'results.trx'
    document = ET.parse(source)
    counters = document.find('.//t:Counters', ns).attrib
    results = document.findall('.//t:UnitTestResult', ns)
    shutil.copyfile(source, target / (project + '.trx'))
    items = ''.join('<li>' + html.escape(test.attrib['testName']) + ' — ' + test.attrib['outcome'] + '</li>' for test in results)
    sections.append(f'<h2>{project}: {counters["passed"]}/{counters["total"]}</h2><ul>{items}</ul>')
mobile = json.loads((target / 'mobile-tests.json').read_text(encoding='utf-8'))
items = ''.join('<li>' + html.escape(test['fullName']) + ' — ' + test['status'] + '</li>' for suite in mobile['testResults'] for test in suite['assertionResults'])
sections.append(f'<h2>Mobile: {mobile["numPassedTests"]}/{mobile["numTotalTests"]}</h2><ul>{items}</ul>')
document = '<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>Passo Seguro — testes executados</title><style>body{font:17px system-ui;background:#f3f6f3;color:#172e28;padding:32px;max-width:1100px;margin:auto}h1,h2{color:#16634f}li{padding:3px}section{background:white;padding:24px;margin:16px 0;border-radius:16px}</style><h1>Passo Seguro — resultados executados</h1><p>Relatório gerado de TRX (.NET) e JSON (Jest). Integração em PostgreSQL 16 real. Não inclui testes em iPhone físico.</p>' + ''.join('<section>' + section + '</section>' for section in sections) + '</html>'
(target / 'test-results.html').write_text(document, encoding='utf-8')
print('Relatório criado a partir dos resultados executados.')
