'use strict';
const registry = window.researchRegistry;
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
let flowFilter = 'all';

function renderCatalogue() {
  const query = search.value.trim().toLowerCase();
  const matches = registry.tokens.filter(token => (!reviewedOnly.checked || token.hasStudy) && (token.symbol.toLowerCase().includes(query) || token.address.includes(query)));
  document.querySelector('#total-count').textContent = registry.tokens.length.toLocaleString('en-US');
  document.querySelector('#result-count').textContent = `${matches.length.toLocaleString('en-US')} contracts${query ? ' matching your search' : ' in the index'}`;
  list.innerHTML = matches.slice(0, visibleCount).map(token => `<button class="token-choice" data-address="${token.address}" aria-current="${selected?.address === token.address}" aria-label="${escape(token.symbol)} ${token.address}${token.hasStudy ? ', investigation available' : ', not assessed'}"><span class="token-icon" aria-hidden="true">${escape(token.symbol.slice(0, 2))}</span><span class="token-label"><strong>${escape(token.symbol)}</strong><small>${short(token.address)}</small></span>${token.hasStudy ? '<span class="token-dot" title="Investigation available"></span>' : '<span class="arrow" aria-hidden="true">›</span>'}</button>`).join('') || '<p class="no-results">No matching contracts. Try a symbol or part of an address.</p>';
  more.hidden = matches.length <= visibleCount;
}

function renderSelection() {
  const address = new URLSearchParams(location.hash.slice(1)).get('token')?.toLowerCase();
  selected = registry.tokens.find(token => token.address === address) || (!address ? registry.tokens[0] : undefined);
  renderCatalogue();
  if (!selected) {
    detail.innerHTML = '<div class="empty"><h1>Contract not in this catalogue</h1><p>The address in this link is not listed in the captured source. Use the token index to select a known contract.</p><a href="./">Return to the registry</a></div>';
    return;
  }
  document.title = `${selected.symbol} · Chainge Postmortem`;
  const study = registry.studies[selected.address];
  detail.innerHTML = `<div class="breadcrumb">Asset registry <span aria-hidden="true">/</span> Fusion <span aria-hidden="true">/</span> ${escape(selected.symbol)}</div><div class="token-heading"><div><p class="eyebrow">02 / Token investigation</p><h1>${escape(selected.symbol)}<small>Fusion</small></h1><p>${study ? escape(study.scope) : 'Contract listed in the public source spreadsheet'}</p></div><div class="heading-meta"><span class="status ${study ? '' : 'neutral'}">${study ? escape(study.status) : 'Not assessed'}</span><p>${study ? 'Research updated 11 Oct 2026' : 'Catalogue captured 11 Oct 2026'}</p></div></div><div class="contract">${selected.address}<button class="copy" data-copy="${selected.address}" aria-label="Copy Fusion contract address">Copy address</button></div><div class="tabs" role="tablist" aria-label="Investigation view">${['overview','vaults','movements'].map(name => `<button id="tab-${name}" role="tab" aria-selected="${tabName === name}" aria-controls="tab-content" tabindex="${tabName === name ? '0' : '-1'}" data-tab="${name}">${name[0].toUpperCase()+name.slice(1)}${study && name !== 'overview' ? `<span>${name === 'vaults' ? study.vaults.length : study.flows.length}</span>` : ''}</button>`).join('')}</div><div id="tab-content" role="tabpanel" aria-labelledby="tab-${tabName}">${study ? renderStudy(study) : renderUnassessed()}</div><div class="source-line">Fusion contract identity comes from <a href="https://docs.google.com/spreadsheets/d/1pyQlI4oGOgaRx-5UWA68meb85KtsnZ98BZrYDrHsqhk/edit?gid=1161132641&range=A${selected.sourceRows[0]}:B${selected.sourceRows[0]}">source row${selected.sourceRows.length > 1 ? 's' : ''} ${selected.sourceRows.join(', ')}</a>. Listing alone does not verify issuer, deployment or custody.</div>`;
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
  const flows = study.flows.filter(flow => flowFilter === 'all' || flow.status === flowFilter);
  const current = flows.find(flow => flow.id === flowId) || flows[0];
  if (current) flowId = current.id;
  return `<div class="section-title"><h2>Follow selected outflows</h2><a href="${repo}research/busd-source-chain-flows.md">Full investigation ↗</a></div><p class="muted">Largest first within these ${study.flows.length} reviewed cases. This is not an exhaustive outflow ranking. “Unmatched” means no verified Fusion burn link; it does not establish a loss.</p><div class="filters"><label for="flow-filter">Explanation</label><select id="flow-filter">${['all','Unmatched','Possible redemption','Possible rebalancing'].map(value=>`<option value="${value}" ${flowFilter === value ? 'selected' : ''}>${value === 'all' ? 'All reviewed cases' : value}</option>`).join('')}</select><span class="muted">Solid path: observed transfers · Dashed: inferred relationship</span></div><div class="flow-layout"><div>${flows.map(flow=>`<button class="flow-choice" data-flow="${flow.id}" aria-pressed="${current?.id === flow.id}"><span><strong>${escape(flow.title)}</strong><small>${escape(flow.date)} · ${escape(flow.chain)}</small><p class="flow-description">Last traced destination: ${escape(flow.endpoint)}</p><span class="status ${flow.status === 'Unmatched' ? '' : 'neutral'}">${escape(flow.status)}</span></span><span class="flow-value">${amount(flow.amount)}<small style="display:block">BUSD</small></span></button>`).join('')}</div>${current ? renderTrace(current) : '<div class="empty">No cases match this filter.</div>'}</div>`;
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
  if (button.dataset.openFlow) { flowId = button.dataset.openFlow; flowFilter = 'all'; tabName = 'movements'; renderSelection(); document.querySelector('#tab-movements').focus(); }
  if (button.dataset.copy) { try { await navigator.clipboard.writeText(button.dataset.copy); button.textContent = 'Copied'; } catch { button.textContent = 'Select address to copy'; } }
});
detail.addEventListener('change', event => { if (event.target.id === 'flow-filter') { flowFilter = event.target.value; renderSelection(); document.querySelector('#flow-filter').focus(); } });
detail.addEventListener('keydown', event => {
  if (event.target.getAttribute('role') !== 'tab' || !['ArrowLeft','ArrowRight','Home','End'].includes(event.key)) return;
  event.preventDefault();
  const tabs = ['overview','vaults','movements'];
  const next = event.key === 'Home' ? 0 : event.key === 'End' ? 2 : (tabs.indexOf(tabName) + (event.key === 'ArrowRight' ? 1 : 2)) % 3;
  tabName = tabs[next]; renderSelection(); document.querySelector(`#tab-${tabName}`).focus();
});
window.addEventListener('hashchange', renderSelection);
renderSelection();
