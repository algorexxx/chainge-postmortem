'use strict';
const registry = JSON.parse(document.querySelector('#registry-data').textContent);
const detail = document.querySelector('#detail');
const list = document.querySelector('#token-list');
const search = document.querySelector('#token-search');
const reviewedOnly = document.querySelector('#reviewed-only');
const more = document.querySelector('#load-more');
const repo = 'https://github.com/algorexxx/chainge-postmortem/blob/main/';
const escape = value => String(value ?? '').replace(/[&<>"']/g, character => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[character]));
const short = address => address.slice(0, 8) + '…' + address.slice(-6);
const format = value => { const [whole, fraction = ''] = String(value).split('.'); return whole.replace(/\B(?=(\d{3})+(?!\d))/g, ',') + (fraction ? '.' + fraction.slice(0, 2) : ''); };
const amount = value => `<span title="${escape(value)}">${String(value).startsWith('0.00') && /[1-9]/.test(value) ? '&lt;0.01' : format(value)}</span>`;
let selected;
let tabName = 'overview';
let visibleCount = 50;
let flowId = 'jan-204k';
let flowFilter = 'hide-redemptions';
let flowSort = 'review';
let flowMinimum = '0';
let movementView = 'terminal';
let ledgerDirection = 'outgoing';
let ledgerClassification = 'hide-redemptions';
let ledgerChain = 'all';
let ledgerToken = 'all';
let ledgerDestination = '';
let ledgerCount = 50;
let ledgerId = '';

function renderCatalogue() {
  const query = search.value.trim().toLowerCase();
  const matches = registry.tokens.filter(token => (!reviewedOnly.checked || token.hasStudy) && (token.symbol.toLowerCase().includes(query) || token.address.includes(query)));
  document.querySelector('#total-count').textContent = registry.tokens.length.toLocaleString('en-US');
  document.querySelector('#result-count').textContent = `${matches.length.toLocaleString('en-US')} contracts${query ? ' matching your search' : ' in the index'}`;
  list.innerHTML = matches.slice(0, visibleCount).map(token => `<button class="token-choice" data-address="${token.address}" aria-current="${selected?.address === token.address}" aria-label="${escape(token.symbol)} ${token.address}${token.hasStudy ? ', investigation available' : ', not assessed'}"><span class="token-icon" aria-hidden="true">${escape(token.symbol.slice(0, 2))}</span><span class="token-label"><strong>${escape(token.symbol)}</strong><small>${short(token.address)}</small></span>${token.hasStudy ? '<span class="token-dot" title="Investigation available"></span>' : '<span class="arrow" aria-hidden="true">›</span>'}</button>`).join('') || '<p class="no-results">No matching contracts. Try a symbol or part of an address.</p>';
  more.hidden = matches.length <= visibleCount;
}

function renderSelection() {
  if (location.hash === '#timeline') {
    selected = undefined;
    renderCatalogue();
    document.title = 'Service timeline · Chainge Postmortem';
    detail.innerHTML = renderTimeline();
    return;
  }
  const address = new URLSearchParams(location.hash.slice(1)).get('token')?.toLowerCase();
  selected = registry.tokens.find(token => token.address === address) || (!address ? registry.tokens[0] : undefined);
  renderCatalogue();
  if (!selected) {
    detail.innerHTML = '<div class="empty"><h1>Contract not in this catalogue</h1><p>The address in this link is not listed in the captured source. Use the token index to select a known contract.</p><a href="./">Return to the registry</a></div>';
    return;
  }
  document.title = `${selected.symbol} · Chainge Postmortem`;
  const study = registry.studies[selected.address];
  detail.innerHTML = `<div class="breadcrumb">Asset registry <span aria-hidden="true">/</span> Fusion <span aria-hidden="true">/</span> ${escape(selected.symbol)}</div><div class="token-heading"><div><p class="eyebrow">02 / Token investigation</p><h1>${escape(selected.symbol)}<small>Fusion</small></h1><p>${study ? escape(study.scope) : 'Contract listed in the public source spreadsheet'}</p></div><div class="heading-meta"><span class="status ${study ? '' : 'neutral'}">${study ? escape(study.status) : 'Not assessed'}</span><p>${study ? 'Research updated 11 Oct 2026' : 'Catalogue captured 11 Oct 2026'}</p></div></div><div class="contract">${selected.address}<button class="copy" data-copy="${selected.address}" aria-label="Copy Fusion contract address">Copy address</button></div><div class="tabs" role="tablist" aria-label="Investigation view">${['overview','vaults','movements'].map(name => `<button id="tab-${name}" role="tab" aria-selected="${tabName === name}" aria-controls="tab-content" tabindex="${tabName === name ? '0' : '-1'}" data-tab="${name}">${name[0].toUpperCase()+name.slice(1)}${study && name !== 'overview' ? `<span>${name === 'vaults' ? study.vaults.length : (study.automatedMovements?.cases.length ?? study.flows.length)}</span>` : ''}</button>`).join('')}</div><div id="tab-content" role="tabpanel" aria-labelledby="tab-${tabName}">${study ? renderStudy(study) : renderUnassessed()}</div><div class="source-line">Fusion contract identity comes from <a href="https://docs.google.com/spreadsheets/d/1pyQlI4oGOgaRx-5UWA68meb85KtsnZ98BZrYDrHsqhk/edit?gid=1161132641&range=A${selected.sourceRows[0]}:B${selected.sourceRows[0]}">source row${selected.sourceRows.length > 1 ? 's' : ''} ${selected.sourceRows.join(', ')}</a>. Listing alone does not verify issuer, deployment or custody.</div>`;
}

function renderUnassessed() {
  const titles = {overview:'This token has not been assessed',vaults:'No vault assessment yet',movements:'No movements reviewed yet'};
  return `<div class="empty"><div class="empty-icon" aria-hidden="true">◌</div><h2>${titles[tabName]}</h2><p>The catalogue provides a starting address. Supply, source-chain mappings and custody relationships still need independent verification. No findings is not the same as no assets.</p><a class="primary" href="#token=0x9d7005f9175856bda5deb153f86a11153d9d0fa7">Explore the BUSD investigation</a></div>`;
}

function renderStudy(study) {
  if (tabName === 'vaults') return renderVaults(study);
  if (tabName === 'movements') return renderMovements(study);
  return `<div class="section-title"><h2>Issuance & backing</h2><a href="${study.assessment}">Read the assessment ↗</a></div><div class="metrics"><div class="metric"><div class="metric-label">Newer token supply · BUSD</div><div class="metric-value">${amount(study.supply)}</div><p>This contract only. Not total outstanding claims across representations.</p><span class="tag">Observed on Fusion · 7 Oct 2025</span></div><div class="metric"><div class="metric-label">Verified total vault backing</div><div class="metric-value unknown">Not established</div><p>Candidate holdings exist, but the custody inventory is incomplete.</p><span class="tag">Source chains · Oct 2026</span></div><div class="metric"><div class="metric-label">Backing / outstanding claims</div><div class="metric-value unknown">Unavailable</div><p>A common cutoff and validated outstanding-token total are required.</p><span class="tag">Reconciliation open</span></div></div><div class="notice"><strong>These snapshots cannot be compared as a reserve ratio.</strong>Fusion state is retained from October 2025. Source-chain balances were observed in October 2026. Legacy representations and migration adjustments remain unresolved.</div><section class="panel"><div class="section-title"><h2>Fusion issuance ledger</h2><a href="${study.ledgerSource}">125 mint / burn events ↗</a></div><p>Newer BUSD representation only. Includes migration-related issuance.</p><div class="ledger">${[['Gross minted',study.minted],['Gross burned',study.burned],['Retained supply',study.supply]].map(([label,value])=>`<div><small>${label} · BUSD</small><strong>${amount(value)}</strong><div class="amount-exact">${value}</div></div>`).join('')}</div><p class="footnote">Cutoff: ${escape(study.fusionCutoff)}. Gross minted is not a count of external deposits.</p></section><div class="two-col"><section class="panel feature"><p class="eyebrow">Featured unresolved path</p><h2>January 2024 → Binance 14</h2><p class="big">${amount(study.flows.find(flow => flow.id === 'jan-204k').amount)} <small>BUSD sent from the Ethereum controller</small></p><p>Three verified transfer hops. No established Fusion burn match or replacement-reserve return.</p><button data-open-flow="jan-204k">Follow the movement →</button></section><section class="panel"><h2>What is known</h2><ul class="list-facts"><li><span>Candidate address positions</span><strong>${study.vaults.length} shown</strong></li><li><span>Selected movement cases</span><strong>${study.flows.length} traced</strong></li><li><span>FDUSD follow-up</span><a href="${study.fdusdSource}">No return identified ↗</a></li><li><span>Overall BUSD reconciliation</span><strong>Incomplete</strong></li></ul><p class="footnote">Individual transfers are verified separately from explanations of their purpose.</p></section></div><section class="panel"><div class="section-title"><h2>Observed controller activity</h2></div><p>Gross receipts and payments in returned histories. Includes routing, swaps and internal transfers; these are not unique customer deposits.</p><div class="table-wrap"><table><thead><tr><th>Chain / representation</th><th>History window</th><th class="numeric">Inflow (token units)</th><th class="numeric">Outflow (token units)</th></tr></thead><tbody>${study.activity.map(row=>`<tr><td><strong>${escape(row.chain)}</strong>${escape(row.token)}</td><td>${escape(row.period)}</td><td class="numeric">${amount(row.inflow)}</td><td class="numeric">${amount(row.outflow)}</td></tr>`).join('')}</tbody></table></div><p class="footnote">Controller: <code>${escape(study.activity[0].address)}</code>. Amounts displayed to two decimals; full values appear on hover. Do not add chain totals as unique capital.</p><a class="evidence-link" href="${repo}research/busd-source-chain-flows.md">History coverage and exact amounts ↗</a></section>`;
}

function renderVaults(study) {
  return `<div class="section-title"><h2>Candidate vaults & operating addresses</h2><a href="${study.assessment}">Custody assessment ↗</a></div><div class="notice"><strong>${study.vaults.length} selected positions, not a complete reserve inventory.</strong>Roles are chain-specific. A configured liquidity address is not proof of segregated backing. No total is calculated across these positions.</div><section class="panel table-wrap"><table><thead><tr><th>Chain / token</th><th>Address & role</th><th class="numeric">Observed balance (token units)</th><th>Evidence / cutoff</th></tr></thead><tbody>${study.vaults.map(v=>`<tr><td><strong>${escape(v.chain)}</strong>${escape(v.token)}</td><td class="address-cell"><strong>${escape(v.role)}</strong><code>${v.address}</code></td><td class="numeric"><strong>${v.balance === '0' ? '0' : amount(v.balance)}</strong>${v.balance !== '0' ? `<div class="amount-exact">${v.balance}</div>` : ''}</td><td class="vault-note">${escape(v.note)}<div class="footnote">${escape(v.cutoff)}</div><a class="evidence-link" href="${v.source}">Role and balance evidence ↗</a></td></tr>`).join('')}</tbody></table></section><p class="muted">A zero balance is an observation at a stated cutoff. An unknown balance would be shown as unknown. Small nonzero balances retain their exact value.</p>`;
}

function renderMovements(study) {
  const switcher = study.automatedMovements ? `<div class="filters"><label for="movement-view">View</label><select id="movement-view"><option value="terminal" ${movementView === 'terminal' ? 'selected' : ''}>Closing transfers</option><option value="ledger" ${movementView === 'ledger' ? 'selected' : ''}>Full ledger · ${study.automatedMovements.cases.length} movements</option><option value="reviewed" ${movementView === 'reviewed' ? 'selected' : ''}>Traced cases · ${study.flows.length}</option></select></div>` : '';
  return switcher + (study.automatedMovements && movementView === 'terminal' ? renderTerminalReview(study.automatedMovements) : study.automatedMovements && movementView === 'ledger' ? renderMovementLedger(study.automatedMovements) : renderReviewedMovements(study));
}

function renderReviewedMovements(study) {
  const flows = study.flows.filter(flow => flowFilter === 'all' || (flowFilter === 'hide-redemptions' ? flow.status !== 'Possible redemption' : flow.status === flowFilter))
    .filter(flow => tokenUnits(flow.amount) >= tokenUnits(flowMinimum))
    .sort(compareFlows);
  const current = flows.find(flow => flow.id === flowId) || flows[0];
  if (current) flowId = current.id;
  return `<div class="section-title"><h2>Reviewed movement cases</h2><a href="${repo}research/busd-source-chain-flows.md">Full investigation ↗</a></div><p class="muted">Showing ${flows.length} of ${study.flows.length} reviewed cases; ${study.flows.length - flows.length} hidden by the current filters. Amounts are token units, not current USD values. Date ordering uses the latest date for grouped cases; ${study.flows.filter(flow => flow.status === 'Possible redemption').length} are possible redemptions, hidden by default. Default order: unmatched first, then newest, then largest. This is a review priority, not evidence of wrongdoing or an exhaustive outflow ranking. “Unmatched” means no verified Fusion burn link; it does not establish a loss.</p><div class="filters"><label for="flow-filter">Explanation</label><select id="flow-filter">${['hide-redemptions','all','Unmatched','Possible redemption','Possible rebalancing'].map(value=>`<option value="${value}" ${flowFilter === value ? 'selected' : ''}>${value === 'hide-redemptions' ? 'Hide possible redemptions' : value === 'all' ? 'All reviewed cases' : value}</option>`).join('')}</select><label for="flow-sort">Sort</label><select id="flow-sort">${[['review','Unmatched first · newest'],['newest','Newest first'],['largest','Largest first']].map(([value,label])=>`<option value="${value}" ${flowSort === value ? 'selected' : ''}>${label}</option>`).join('')}</select><label for="flow-minimum">Minimum (token units)</label><select id="flow-minimum">${['0','100','1000','10000','100000','1000000'].map(value=>`<option value="${value}" ${flowMinimum === value ? 'selected' : ''}>${value === '0' ? 'No minimum' : format(value)}</option>`).join('')}</select><button class="reset-filters" data-reset-flows="true">Reset filters</button><span class="muted">Solid path: observed transfers · Dashed: inferred relationship</span></div><div class="flow-layout"><div>${flows.map(flow=>`<button class="flow-choice" data-flow="${flow.id}" aria-pressed="${current?.id === flow.id}"><span><strong>${escape(flow.title)}</strong><small>${escape(flow.date)} · ${escape(flow.chain)}</small><p class="flow-description">Last traced destination: ${escape(flow.endpoint)}</p><span class="status ${flow.status === 'Unmatched' ? '' : 'neutral'}">${escape(flow.status)}</span></span><span class="flow-value">${amount(flow.amount)}<small style="display:block">BUSD</small></span></button>`).join('')}</div>${current ? renderTrace(current) : '<div class="empty">No cases match this filter.</div>'}</div>`;
}

function compareFlows(a, b) {
  const priority = flowSort === 'review' ? Number(b.status === 'Unmatched') - Number(a.status === 'Unmatched') : 0;
  const dateA = (a.date.match(/\d{4}-\d{2}-\d{2}/g) || []).sort().at(-1) || '';
  const dateB = (b.date.match(/\d{4}-\d{2}-\d{2}/g) || []).sort().at(-1) || '';
  const byDate = dateB.localeCompare(dateA);
  const byAmount = tokenUnits(a.amount) > tokenUnits(b.amount) ? -1 : tokenUnits(a.amount) < tokenUnits(b.amount) ? 1 : 0;
  return priority || (flowSort === 'largest' ? byAmount || byDate : byDate || byAmount) || a.id.localeCompare(b.id);
}

function tokenUnits(value) {
  const [whole, fraction = ''] = value.split('.');
  return BigInt(whole) * 10n ** 18n + BigInt(fraction.padEnd(18, '0'));
}

function renderTrace(flow) {
  return `<section class="trace-panel" aria-label="Selected movement path"><p class="eyebrow">03 / Movement path</p><h3>${escape(flow.title)}</h3><p>${escape(flow.explanation)}</p><ol class="trace-path">${flow.steps.map((step,index)=>`<li class="${flow.steps[index+1]?.kind === 'inferred' ? 'inferred' : ''}"><span class="chain">${escape(step.chain)}${step.kind === 'inferred' ? ' · relationship inferred' : ''}</span><strong>${escape(step.label)}</strong><code>${step.address}</code><div class="amount-exact">${step.amount} ${step.chain === 'Fusion' ? 'Fusion BUSD' : 'BUSD'}</div><time>${escape(step.time)}</time>${step.note ? `<p>${escape(step.note)}</p>` : ''}${step.transaction ? `<a href="${step.transaction}">View transaction ↗</a>` : ''}</li>`).join('')}</ol><div class="trace-unknown">${flow.status === 'Possible redemption' ? 'The burn/payment relationship is inferred. Later combined-balance transfers require the accompanying report.' : flow.status === 'Possible rebalancing' ? 'The exchange-internal connection is not visible on-chain.' : 'Tracing stops here. The credited exchange account, retained funds and later withdrawals are unknown.'}</div><a class="evidence-link" href="${flow.source}">Open evidence and exact transaction history ↗</a></section>`;
}

search.addEventListener('input', () => { visibleCount = 50; renderCatalogue(); });
reviewedOnly.addEventListener('change', () => { visibleCount = 50; renderCatalogue(); });
more.addEventListener('click', () => { visibleCount += 50; renderCatalogue(); });
list.addEventListener('click', event => { const button = event.target.closest('[data-address]'); if (button) { tabName = 'overview'; const next = '#token=' + button.dataset.address; if (location.hash === next) { renderSelection(); } else { location.hash = next; } } });
detail.addEventListener('click', async event => {
  const button = event.target.closest('button');
  if (!button) return;
  if (button.dataset.tab) { tabName = button.dataset.tab; renderSelection(); document.querySelector(`#tab-${tabName}`).focus(); }
  if (button.dataset.flow) { flowId = button.dataset.flow; renderSelection(); document.querySelector(`[data-flow="${flowId}"]`).focus(); }
  if (button.dataset.openFlow) { movementView = 'reviewed'; flowId = button.dataset.openFlow; flowFilter = 'all'; flowMinimum = '0'; tabName = 'movements'; renderSelection(); document.querySelector('#tab-movements').focus(); }
  if (button.dataset.ledgerId) { ledgerId = button.dataset.ledgerId; renderSelection(); document.querySelector('.ledger-detail').scrollIntoView({block:'nearest'}); }
  if (button.dataset.ledgerMore) { ledgerCount += 50; renderSelection(); document.querySelector('[data-ledger-more]')?.focus(); }
  if (button.dataset.destination) { ledgerDestination = button.dataset.destination; ledgerChain = button.dataset.chain; ledgerToken = button.dataset.token; ledgerDirection = 'outgoing'; ledgerClassification = 'all'; flowMinimum = '0'; ledgerCount = 50; ledgerId = ''; renderSelection(); document.querySelector('#ledger-destination').focus(); }
  if (button.dataset.resetLedger) { ledgerDirection = 'outgoing'; ledgerClassification = 'hide-redemptions'; ledgerChain = 'all'; ledgerToken = 'all'; ledgerDestination = ''; flowMinimum = '0'; flowSort = 'review'; ledgerCount = 50; ledgerId = ''; renderSelection(); document.querySelector('#ledger-classification').focus(); }
  if (button.dataset.resetFlows) { flowFilter = 'hide-redemptions'; flowSort = 'review'; flowMinimum = '0'; flowId = ''; renderSelection(); document.querySelector('#flow-filter').focus(); }
  if (button.dataset.copy) { try { await navigator.clipboard.writeText(button.dataset.copy); button.textContent = 'Copied'; } catch { button.textContent = 'Select address to copy'; } }
});
detail.addEventListener('input', event => {
  if (event.target.id !== 'ledger-destination') return;
  const position = event.target.selectionStart;
  ledgerDestination = event.target.value.toLowerCase();
  ledgerCount = 50; ledgerId = ''; renderSelection();
  const input = document.getElementById('ledger-destination');
  input.focus(); input.setSelectionRange(position, position);
});
detail.addEventListener('change', event => {
  const id = event.target.id;
  if (id === 'movement-view') { movementView = event.target.value; if (movementView === 'reviewed' && !['review','newest','largest'].includes(flowSort)) flowSort = 'review'; renderSelection(); document.getElementById(id).focus(); return; }
  if (id.startsWith('ledger-')) {
    if (id === 'ledger-direction') ledgerDirection = event.target.value;
    if (id === 'ledger-classification') ledgerClassification = event.target.value;
    if (id === 'ledger-chain') { ledgerChain = event.target.value; ledgerToken = 'all'; }
    if (id === 'ledger-token') ledgerToken = event.target.value;
    if (id === 'ledger-destination') ledgerDestination = event.target.value.trim().toLowerCase();
    if (id === 'ledger-sort') flowSort = event.target.value;
    if (id === 'ledger-minimum') flowMinimum = event.target.value;
    ledgerCount = 50; ledgerId = ''; renderSelection(); document.getElementById(id).focus(); return;
  }
  if (!['flow-filter', 'flow-sort', 'flow-minimum'].includes(id)) return;
  if (id === 'flow-filter') flowFilter = event.target.value;
  if (id === 'flow-sort') flowSort = event.target.value;
  if (id === 'flow-minimum') flowMinimum = event.target.value;
  flowId = '';
  renderSelection();
  document.getElementById(id).focus();
});
detail.addEventListener('keydown', event => {
  if (event.target.getAttribute('role') !== 'tab' || !['ArrowLeft','ArrowRight','Home','End'].includes(event.key)) return;
  event.preventDefault();
  const tabs = ['overview','vaults','movements'];
  const next = event.key === 'Home' ? 0 : event.key === 'End' ? 2 : (tabs.indexOf(tabName) + (event.key === 'ArrowRight' ? 1 : 2)) % 3;
  tabName = tabs[next]; renderSelection(); document.querySelector(`#tab-${tabName}`).focus();
});
window.addEventListener('hashchange', renderSelection);
renderSelection();

function rawUnits(raw, decimals) {
  const negative = String(raw).startsWith('-');
  const digits = String(raw).replace(/^-/, '').padStart(decimals + 1, '0');
  return (negative ? '-' : '') + (decimals ? digits.slice(0, -decimals) + '.' + digits.slice(-decimals) : digits);
}

function comparableRaw(raw, decimals) {
  return BigInt(raw) * 10n ** BigInt(255 - decimals);
}

function compareInteger(a, b) { return a > b ? -1 : a < b ? 1 : 0; }
function destinationKey(chain, token, address) { return [chain, token, address].join(':'); }
function classificationLabel(value) { return value.replaceAll('-', ' '); }
function depletionLabel(value) { return value === null ? 'Unavailable' : (value / 100).toFixed(2) + '%'; }

function ledgerSelect(id, label, options, selectedValue) {
  return `<label for="${id}">${label}</label><select id="${id}">${options.map(([value,text]) => `<option value="${escape(value)}" ${selectedValue === value ? 'selected' : ''}>${escape(text)}</option>`).join('')}</select>`;
}

function renderMovementLedger(report) {
  const destinations = new Map(report.destinations.map(d => [destinationKey(d.chain, d.token, d.address), d]));
  const minimum = comparableRaw(flowMinimum, 0);
  const matches = report.cases.filter(c => (ledgerDirection === 'all' || c.review.direction === ledgerDirection)
    && (ledgerClassification === 'all' || (ledgerClassification === 'hide-redemptions' ? c.review.classification !== 'possible-redemption' : c.review.classification === ledgerClassification))
    && (ledgerChain === 'all' || c.chain === ledgerChain)
    && (ledgerToken === 'all' || c.token === ledgerToken)
    && (!ledgerDestination || c.to.includes(ledgerDestination))
    && comparableRaw(c.rawAmount, c.decimals) >= minimum);
  matches.sort((a,b) => {
    const priority = flowSort === 'review' ? Number(b.review.classification === 'unmatched') - Number(a.review.classification === 'unmatched') : 0;
    const byDate = (b.timestamp ?? '').localeCompare(a.timestamp ?? '');
    const byAmount = compareInteger(comparableRaw(a.rawAmount,a.decimals),comparableRaw(b.rawAmount,b.decimals));
    const byDepletion = (b.depletionBasisPoints ?? -1) - (a.depletionBasisPoints ?? -1);
    const aDest = destinations.get(destinationKey(a.chain,a.token,a.to));
    const bDest = destinations.get(destinationKey(b.chain,b.token,b.to));
    const byDestination = compareInteger(comparableRaw(aDest?.unmatchedOutflowRaw ?? '0',a.decimals),comparableRaw(bDest?.unmatchedOutflowRaw ?? '0',b.decimals));
    return priority || (flowSort === 'largest' ? byAmount : flowSort === 'depletion' ? byDepletion : flowSort === 'destination' ? byDestination : byDate) || byDate || byAmount || a.id.localeCompare(b.id);
  });
  const visible = matches.slice(0,ledgerCount);
  const current = visible.find(c => c.id === ledgerId) || visible[0];
  if (current) ledgerId = current.id;
  const classifications = [...new Set(report.cases.map(c => c.review.classification))].sort();
  return `<div class="section-title"><h2>Automated movement ledger</h2><span class="status neutral">Provisional classifications</span></div>
    <p class="muted">${report.cases.length.toLocaleString()} retained movements · ${report.cases.filter(c=>c.review.direction === 'outgoing' && c.review.classification === 'unmatched').length} unmatched outflows. Showing ${visible.length} of ${matches.length} matching movements; ${report.cases.length - matches.length} excluded by filters. Possible redemptions are hidden by default. Unmatched means no pairing found under the normal rules, not proven loss.</p>
    <div class="filters ledger-filters">
    ${ledgerSelect('ledger-direction','Direction',[['outgoing','Outflows'],['incoming','Inflows'],['internal','Internal'],['all','All movements']],ledgerDirection)}
    ${ledgerSelect('ledger-classification','Explanation',[['hide-redemptions','Hide possible redemptions'],['all','All classifications'],...classifications.map(c=>[c,classificationLabel(c)])],ledgerClassification)}
    ${ledgerSelect('ledger-chain','Chain',[['all','All chains'],...[...new Set(report.cases.map(c=>c.chain))].sort().map(c=>[c,c])],ledgerChain)}
    ${ledgerSelect('ledger-token','Token contract',[['all','All representations'],...[...new Set(report.cases.filter(c=>ledgerChain === 'all' || c.chain === ledgerChain).map(c=>c.token))].sort().map(c=>[c,c])],ledgerToken)}
    ${ledgerSelect('ledger-sort','Sort',[['review','Unmatched first · newest'],['newest','Newest first'],['largest','Largest first'],['depletion','Balance reduction first'],['destination','Destination unmatched total']],flowSort)}
    ${ledgerSelect('ledger-minimum','Minimum (token units)',['0','100','1000','10000','100000','1000000'].map(v=>[v,v === '0' ? 'No minimum' : format(v)]),flowMinimum)}
    <label for="ledger-destination">Recipient address</label><input id="ledger-destination" value="${escape(ledgerDestination)}" placeholder="Full address or fragment" spellcheck="false"><button class="reset-filters" data-reset-ledger="true">Reset filters</button></div>
    <p class="footnote">Amounts are token units, not current USD values. Balance reduction = payment / reconstructed sender balance before payment, assuming a zero opening balance. It is conditional on history coverage; impossible ratios are unavailable. Sorting is a review aid, not a misconduct score.</p>
    ${renderDestinations(report.destinations)}
    <div class="flow-layout"><div>${visible.map(c=>`<button class="flow-choice" data-ledger-id="${escape(c.id)}" aria-pressed="${current?.id === c.id}"><span><strong>${escape(classificationLabel(c.review.classification))}</strong><small>${escape(c.timestamp ?? 'Unknown date')} · ${escape(c.chain)} · ${escape(c.review.direction)}</small><p class="flow-description">${escape(short(c.from))} → ${escape(short(c.to))}</p><small>Reconstructed balance reduction: ${depletionLabel(c.depletionBasisPoints)}</small>${c.review.flags.length ? `<p class="flow-description">${c.review.flags.map(f=>escape(classificationLabel(f))).join(' · ')}</p>` : ''}</span><span class="flow-value">${amount(rawUnits(c.rawAmount,c.decimals))}<small style="display:block">${escape(report.asset)}</small></span></button>`).join('')}${matches.length > visible.length ? '<button class="load-more" data-ledger-more="true">Show 50 more movements</button>' : ''}</div>${current ? renderLedgerDetail(current,destinations.get(destinationKey(current.chain,current.token,current.to)),report) : '<div class="empty">No movements match these filters. Reset filters to restore the default view.</div>'}</div>`;
}

function renderDestinations(destinations) {
  const ranked = destinations.filter(d => ledgerChain === 'all' || d.chain === ledgerChain).slice().sort((a,b)=>compareInteger(comparableRaw(a.unmatchedOutflowRaw,a.decimals),comparableRaw(b.unmatchedOutflowRaw,b.decimals)) || b.count-a.count).slice(0,10);
  return `<details class="panel destination-panel"><summary>Top destinations by unmatched outflow total</summary><p>Gross transfers across all retained outflows, grouped separately by chain, token and recipient. Includes small transfers regardless of the amount filter. Only the chain filter applies here. No netting against returns; these totals do not establish ownership, unique capital or losses. Click a recipient to show its outflows.</p><div class="table-wrap"><table><thead><tr><th>Chain / token / recipient</th><th>Outflow count</th><th class="numeric">Gross (token units)</th><th class="numeric">Unmatched (token units)</th></tr></thead><tbody>${ranked.map(d=>`<tr><td>${escape(d.chain)}<code class="address-text">Token: ${escape(d.token)}</code><button class="copy" data-destination="${escape(d.address)}" data-chain="${escape(d.chain)}" data-token="${escape(d.token)}">${escape(d.address)}</button></td><td>${d.count} (${d.unmatchedCount} unmatched)</td><td class="numeric">${amount(rawUnits(d.grossOutflowRaw,d.decimals))}</td><td class="numeric">${amount(rawUnits(d.unmatchedOutflowRaw,d.decimals))}</td></tr>`).join('')}</tbody></table></div></details>`;
}

function renderLedgerDetail(c, destination, report) {
  const delayed = report.delayedReview?.find(m => m.sourceEventId === c.id);
  const traced = registry.studies[selected.address].flows.filter(flow => flow.steps.some(step => step.transaction?.endsWith('/' + c.transaction)));
  const explorers = {BSC:'https://bscscan.com/tx/',Ethereum:'https://etherscan.io/tx/',Avalanche:'https://snowtrace.io/tx/'};
  return `<section class="trace-panel ledger-detail" aria-label="Selected automated movement"><p class="eyebrow">Automated case · review required</p><h3>${escape(classificationLabel(c.review.classification))}</h3><p>One source-chain transfer event. The recipient shown is the immediate recipient, not a traced final destination.</p><code class="address-text">${escape(c.id)}</code><p>${escape(c.timestamp ?? 'Unknown date')} · ${escape(c.chain)}</p><p><strong>${amount(rawUnits(c.rawAmount,c.decimals))} ${escape(report.asset)}</strong></p><code class="address-text">Token: ${escape(c.token)}<br>From: ${escape(c.from)}<br>To: ${escape(c.to)}</code>${explorers[c.chain] ? `<a class="evidence-link" href="${explorers[c.chain]}${encodeURIComponent(c.transaction)}">View source transaction ↗</a>` : ''}<p>Receipt verification: ${c.receiptVerified ? 'verified' : 'not verified'}.</p><h3>Reconstructed sender balance</h3><p>${c.review.direction === 'outgoing' ? `Before: ${c.review.reconstructedBalanceBeforeRaw === null ? 'Unavailable' : amount(rawUnits(c.review.reconstructedBalanceBeforeRaw,c.decimals))}<br>After: ${c.review.reconstructedBalanceAfterRaw === null ? 'Unavailable' : amount(rawUnits(c.review.reconstructedBalanceAfterRaw,c.decimals))}<br>Reduction: ${depletionLabel(c.depletionBasisPoints)}. Conditional on retained history and zero opening balance.` : 'Balance-depletion comparison applies to outflows only.'}</p>
    ${destination ? `<h3>Same recipient · ${escape(c.chain)}</h3><p>Across the complete retained outflow history for this token:<br>${destination.count} payments · ${amount(rawUnits(destination.grossOutflowRaw,destination.decimals))} gross token units.<br>${destination.unmatchedCount} unmatched · ${amount(rawUnits(destination.unmatchedOutflowRaw,destination.decimals))} token units.<br>Includes payments hidden by filters. Returns are not deducted.</p>` : ''}
    ${traced.length ? `<h3>Related traced cases</h3><p>These reports may contain explanations beyond the automated matching rules.</p>${traced.map(flow=>`<button class="reset-filters" data-open-flow="${escape(flow.id)}">${escape(flow.title)} →</button>`).join('')}` : ''}
    <h3>Candidate relationships</h3><p>${c.review.candidates.length} Fusion candidates · ${c.review.sourceCandidates.length} source-chain candidates. Candidates are hypotheses; competing candidates are not independently settled payments.</p>
    ${c.review.candidates.map(p=>`<div class="candidate"><code class="address-text">${escape(p.fusionEventId)}</code><p>${escape(p.rule)} · delay ${escape(p.delay)} · fee hypothesis ${escape(p.feeRawHypothesis)} raw units · ${p.competing ? 'competing' : 'no competing normal candidate'}</p></div>`).join('')}
    ${c.review.sourceCandidates.map(p=>`<div class="candidate"><code class="address-text">${escape(p.otherEventId)}</code><p>Delay ${escape(p.delay)} · fee ${escape(p.feeRawHypothesis)} raw units · ${p.competing ? 'competing' : 'no competing normal candidate'}</p></div>`).join('')}
    <h3>Additional delayed-review candidates</h3><p>${delayed?.candidates.length ?? 0} Fusion candidates · ${delayed?.sourceCandidates.length ?? 0} source-chain candidates. Wider-window hypotheses are kept separate and do not change the normal classification.</p>${delayed ? `<pre class="json-evidence">${escape(JSON.stringify({fusion:delayed.candidates,sourceChain:delayed.sourceCandidates},null,2))}</pre>` : ''}
    <details><summary>Receipt exchange evidence (${c.review.exchangeEvidence.length})</summary><pre class="json-evidence">${escape(JSON.stringify(c.review.exchangeEvidence,null,2))}</pre></details>
    <h3>Retained evidence</h3>${c.evidence.map(path=>`<a class="evidence-link address-text" href="${repo}${path.split('/').map(encodeURIComponent).join('/')}">${escape(path)} ↗</a>`).join('')}<details><summary>Report provenance and limitations</summary><p>Configuration SHA-256: <code class="address-text">${escape(report.configurationSha256)}</code></p><pre class="json-evidence">${escape(JSON.stringify(report.limitations,null,2))}</pre></details></section>`;
}

function terminalPayment(id, byId, asset) {
  const c = byId.get(id);
  return c ? `<strong>${amount(rawUnits(c.rawAmount,c.decimals))} ${escape(asset)}</strong><small>${escape(c.timestamp ?? 'Unknown date')}<br>${escape(classificationLabel(c.review.classification))} · ${escape(c.review.direction)}</small>` : '<strong>None observed</strong>';
}

function renderTerminalReview(report) {
  const byId = new Map(report.cases.map(c=>[c.id,c]));
  const ids = new Set((report.terminal ?? []).flatMap(s=>[s.lastSubstantialOutflowId,s.finalOutflowId]).filter(Boolean));
  const cases = [...ids].map(id=>byId.get(id)).filter(Boolean).sort((a,b)=>(b.timestamp ?? '').localeCompare(a.timestamp ?? '') || a.id.localeCompare(b.id));
  return `<div class="section-title"><h2>Closing transfers</h2><span class="status neutral">${cases.length} transfers · newest first</span></div>
    <p class="muted">The last large withdrawal and final payment from each tracked vault, together in one list. Open a card for evidence or choose Full ledger for every retained movement.</p>
    <div class="closing-list">${cases.map(c=>renderClosingTransfer(c,report)).join('') || '<div class="empty">No closing transfers available.</div>'}</div>
    <details class="panel"><summary>How this shortlist is selected</summary><p>Large means at least ${amount(rawUnits(report.terminalPolicy.largeOutflowRaw,report.terminalPolicy.decimals))} ${escape(report.asset)}. Final means last in retained evidence, not proof that activity ended. A payment that meets both criteria appears once. Small final payments and internal transfers are included. Amounts are token units, not current dollar values. An unmatched transfer is not proof of loss.</p></details>
    ${renderTerminalConvergence(report,byId)}`;
}

function renderClosingTransfer(c, report) {
  const traced = registry.studies[selected.address].flows.filter(flow=>flow.steps.some(step=>step.transaction?.endsWith('/' + c.transaction)));
  const summaries = report.terminal.filter(s=>s.finalOutflowId === c.id || s.lastSubstantialOutflowId === c.id);
  const final = summaries.some(s=>s.finalOutflowId === c.id);
  const large = summaries.some(s=>s.lastSubstantialOutflowId === c.id);
  const label = c.review.direction === 'internal' ? 'Internal transfer' : c.review.classification === 'unmatched' ? 'Unmatched by script' : classificationLabel(c.review.classification);
  const destination = report.destinations.find(d=>d.chain === c.chain && d.token === c.token && d.address === c.to);
  const date = c.timestamp ? new Date(c.timestamp).toLocaleDateString('en-GB',{day:'numeric',month:'short',year:'numeric',timeZone:'UTC'}) : 'Unknown date';
  return `<details class="closing-card"><summary><span class="closing-date">${escape(date)}<small>${escape(c.chain)}</small></span><span class="closing-amount">${amount(rawUnits(c.rawAmount,c.decimals))} <small>${escape(report.asset)}</small></span><span class="closing-explanation"><strong>${escape(traced.length ? traced.map(f=>f.status).join(' · ') : label)}</strong><small>${traced.length ? 'Traced case available' : `To ${escape(short(c.to))}`} · ${large && final ? 'Last large & final payment' : large ? 'Last large withdrawal' : 'Final payment'}</small></span><span class="closing-toggle" aria-hidden="true">＋</span></summary>
    <div class="closing-body">${traced.map(flow=>`<h3>${escape(flow.title)}</h3><p>${escape(flow.explanation)}</p><p><strong>Traced endpoint:</strong> ${escape(flow.endpoint)}</p><button class="reset-filters" data-open-flow="${escape(flow.id)}">Follow the traced path →</button>`).join('') || `<p>${c.review.direction === 'internal' ? 'A transfer between configured operating addresses.' : c.review.classification === 'unmatched' ? 'The script found no match under its normal rules. The purpose of this payment remains unresolved.' : 'This is a provisional automated explanation; inspect the candidate relationships below.'}</p>`}
    <p class="muted">Immediate recipient: <code class="address-text">${escape(c.to)}</code></p>
    <details class="closing-evidence"><summary>Transaction, matching candidates & evidence</summary>${renderLedgerDetail(c,destination,report)}</details>
    <details class="closing-evidence"><summary>Vault balance context</summary>${summaries.map(s=>`<p>${escape(s.chain)} · <code class="address-text">${escape(s.address)}</code></p><p>Last reconstructed balance: <strong>${amount(rawUnits(s.closingBalanceRaw,s.decimals))} ${escape(report.asset)}</strong>. Closing sequence reduction: ${depletionLabel(s.netReductionBasisPoints)}.</p><p>Assumes zero opening balance and depends on retained history. ${s.nonnegativeHistory ? 'No negative balance encountered.' : 'Negative balances occurred; coverage is inconsistent.'} A final zero does not establish a complete vault inventory.</p>`).join('')}</details></div></details>`;
}

function renderTerminalConvergence(report, byId) {
  return `<details class="panel"><summary>Recipient convergence · ${report.sharedTerminalDestinations?.length ?? 0} same-chain groups / ${report.crossChainTerminalRecipients?.length ?? 0} cross-chain address groups</summary><p>Only recipients shared by distinct source addresses on the same chain and token are grouped. This tests direct transfers in the selected sequences; it does not trace later hops or establish who controls an address.</p>${(report.sharedTerminalDestinations ?? []).map(d=>`<h3>${escape(d.chain)} · ${escape(d.address)}</h3><code class="address-text">Token: ${escape(d.token)}</code><p>Sources: ${d.sourceAddresses.map(escape).join(', ')}</p>${d.eventIds.map(id=>`<p><code class="address-text">${escape(id)}</code>${terminalPayment(id,byId,report.asset)}</p>`).join('')}`).join('') || '<p>No shared immediate recipients found within this comparison scope. Later intermediary convergence is not ruled out.</p>'}<h3>Same recipient address across chains</h3><p>Identical address bytes are a lead, not proof of common control. Amounts and transfers stay separate by chain.</p>${(report.crossChainTerminalRecipients ?? []).map(d=>`<code class="address-text">${escape(d.address)}</code>${d.eventIds.map(id=>`<p>${escape(byId.get(id).chain)} · ${escape(short(byId.get(id).from))}<br>${terminalPayment(id,byId,report.asset)}</p>`).join('')}`).join('') || '<p>No cross-chain recipient-address reuse found in these sequences.</p>'}</details>`;
}

function renderTimeline() {
  const timeline = registry.timeline;
  if (!timeline) return '<div class="empty">No timeline available.</div>';
  const coverage = timeline.coverage;
  return `<p class="eyebrow">Project history</p><div class="section-title"><h1>Service timeline</h1><span class="status neutral">${escape(timeline.status)}</span></div>
    <p>Reported disruptions, funding plans and restoration updates from the founder and community admins. Read from oldest to newest, and expand any event to inspect the preserved statements.</p>
    <div class="notice"><strong>Statements are evidence of what was said.</strong>They do not independently prove a blacklist, funding receipt or repayment. No single shutdown date has been established.</div>
    <p class="muted">${timeline.entries.length} milestones · reviewed ${escape(coverage.reviewedDate)} · all publication times in UTC. <a href="${repo}research/service-timeline.md">Method and open questions ↗</a></p>
    <div class="service-timeline">${timeline.entries.map(entry=>renderTimelineEntry(entry,timeline.sources)).join('')}</div>
    <details class="panel"><summary>Source coverage, attribution & limitations</summary><p>Retained export: ${escape(coverage.firstRetainedUtc)} to ${escape(coverage.lastRetainedUtc)} · ${coverage.retainedRecords.toLocaleString()} records. Export completion time was not supplied.</p><p>${escape(coverage.attribution)}</p><p>${escape(coverage.method)}</p><ul>${coverage.limitations.map(item=>`<li>${escape(item)}</li>`).join('')}</ul><p>Private original SHA-256:<code class="address-text">${escape(coverage.rawExportSha256)}</code></p><a href="${repo}registry/timeline.json">Reviewed timeline and excerpts ↗</a></details>`;
}

function renderTimelineEntry(entry, sources) {
  return `<article class="panel timeline-entry"><p class="eyebrow"><time datetime="${escape(entry.publishedUtc)}">${escape(entry.publishedUtc.slice(0,10))}</time> · ${escape(entry.scope)}</p><h2>${escape(entry.title)}</h2><p>${escape(entry.summary)}</p>
    <details><summary>Sources & uncertainty · ${entry.sourceIds.length} statement${entry.sourceIds.length === 1 ? '' : 's'}</summary><p><strong>Event or target time:</strong> ${escape(entry.eventTime)}</p><p><strong>Not established:</strong> ${escape(entry.limitation)}</p>
    ${entry.sourceIds.map(id=>{const source=sources[id];return `<section class="timeline-source"><p><a href="${escape(source.url)}">${escape(source.author)} · message ${source.messageId} ↗</a><br><time datetime="${escape(source.publishedUtc)}">${escape(source.publishedUtc)}</time>${source.lastEditedUtc ? `<br>Last retained edit: ${escape(source.lastEditedUtc)}` : ''}</p>${[source.excerpt,...(source.additionalExcerpts ?? [])].map(excerpt=>`<blockquote>${escape(excerpt)}</blockquote>`).join('')}<p class="footnote">${source.excerptIsCompleteMessage ? 'Complete retained message text.' : 'Selected verbatim excerpt(s); intervening or surrounding text omitted.'} Live message content has not been reverified.</p></section>`;}).join('')}</details></article>`;
}
