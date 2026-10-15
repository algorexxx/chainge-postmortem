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
let flowId = '';
let flowFilter = 'hide-redemptions';
let flowSort = 'review';
let flowMinimum = '0';
let movementView = 'reviewed';
let ledgerAnnouncement = '';
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
  list.innerHTML = matches.slice(0, visibleCount).map(token => `<div class="token-row"><button class="token-choice" data-address="${token.address}" aria-current="${selected?.address === token.address}" aria-label="${escape(token.symbol)} ${token.address}${token.hasStudy ? ', investigation available' : ', not investigated'}"><span class="token-icon" aria-hidden="true">${escape(token.symbol.slice(0, 2))}</span><span class="token-label"><strong>${escape(token.symbol)}</strong><small>${short(token.address)}</small></span>${token.hasStudy ? '<span class="token-dot" title="Investigation available"></span>' : '<span class="arrow" aria-hidden="true">›</span>'}</button>${explorerLink("Fusion", token.address, "↗")}</div>`).join('') || '<p class="no-results">No matching contracts. Try a symbol or part of an address.</p>';
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
  detail.innerHTML = `<div class="breadcrumb">Tokens <span aria-hidden="true">/</span> Fusion <span aria-hidden="true">/</span> ${escape(selected.symbol)}</div><div class="token-heading"><div><p class="eyebrow">Token investigation</p><h1>${escape(selected.symbol)}<small>Fusion</small></h1><p>${study ? 'Vault balances, transfers and supporting evidence.' : 'Listed in the public token spreadsheet. Not yet investigated.'}</p></div><div class="heading-meta"><span class="status ${study ? '' : 'neutral'}">${study ? escape(study.status) : 'Not investigated'}</span><p>${study ? `Research updated ${escape(study.reviewed)}` : 'Catalogue captured 11 Oct 2026'}</p></div></div><div class="contract">${explorerLink("Fusion", selected.address)}<button class="copy" data-copy="${selected.address}" aria-label="Copy Fusion contract address">Copy address</button></div><div class="tabs" role="tablist" aria-label="Investigation view">${['overview','vaults','movements'].map(name => `<button id="tab-${name}" role="tab" aria-selected="${tabName === name}" aria-controls="tab-content" tabindex="${tabName === name ? '0' : '-1'}" data-tab="${name}">${name === 'movements' ? 'Transfers' : name[0].toUpperCase()+name.slice(1)}${study && name !== 'overview' ? `<span>${name === 'vaults' ? study.vaults.length : (study.automatedMovements?.cases.length ?? study.flows.length)}</span>` : ''}</button>`).join('')}</div><div id="tab-content" role="tabpanel" aria-labelledby="tab-${tabName}">${study ? renderStudy(study) : renderUnassessed()}</div><div class="source-line">Fusion contract identity comes from <a href="https://docs.google.com/spreadsheets/d/1pyQlI4oGOgaRx-5UWA68meb85KtsnZ98BZrYDrHsqhk/edit?gid=1161132641&range=A${selected.sourceRows[0]}:B${selected.sourceRows[0]}">source row${selected.sourceRows.length > 1 ? 's' : ''} ${selected.sourceRows.join(', ')}</a>. Listing alone does not verify issuer, deployment or custody.</div>`;
}

function renderUnassessed() {
  const titles = {overview:'This token has not been investigated',vaults:'No vault investigation yet',movements:'No transfers reviewed yet'};
  return `<div class="empty"><div class="empty-icon" aria-hidden="true">◌</div><h2>${titles[tabName]}</h2><p>The catalogue provides a starting address. Supply, source-chain mappings and custody relationships still need independent verification. No findings is not the same as no assets.</p><a class="primary" href="#token=0x9d7005f9175856bda5deb153f86a11153d9d0fa7">Explore the BUSD investigation</a></div>`;
}

function renderStudy(study) {
  if (tabName === 'vaults') return renderVaults(study);
  if (tabName === 'movements') return renderMovements(study);
  return renderVaultComparison(study) + renderFusionStatistics(study) + renderClaimReview(study) + renderAnnouncementContext(study) + `<section class="panel"><h2>Investigation notes</h2><p>${escape(study.scope)}</p>${(study.overviewLinks ?? []).map(link=>`<p><a href="${escape(link.url)}">${escape(link.label)} ↗</a></p>`).join('')}</section>`;
}

function renderFusionStatistics(study) {
  const stats = study.fusionStatistics;
  if (!stats) return '<section class="panel"><h2>Fusion accounting</h2><p>Not yet established. No supply is assumed.</p></section>';
  const token = stats.tokens.find(row => row.address === selected.address);
  const remaining = row => rawUnits((BigInt(row.mintedRaw)-BigInt(row.burnedRaw)).toString(),row.decimals);
  const metrics = token ? [['Total created',rawUnits(token.mintedRaw,token.decimals)],['Total burned',rawUnits(token.burnedRaw,token.decimals)],['Remaining supply',remaining(token)]] : [];
  return `<section class="panel"><div class="section-title"><h2>${escape(study.title)} on Fusion</h2><a href="${escape(stats.source)}">Read the accounting report ↗</a></div>
    <p class="muted">Snapshot: ${escape(stats.referenceTime.replace('T',' ').replace('.000Z',' UTC'))} · <a href="https://fsnex.net/block/${stats.referenceBlock}">Fusion block ${stats.referenceBlock.toLocaleString()} ↗</a></p>
    <p>Selected Fusion token · ${explorerLink('Fusion',selected.address,short(selected.address))}. Amounts below are ${escape(study.title)} token units.</p>
    ${token ? `<div class="metrics">${metrics.map(([label,value])=>`<div class="metric"><div class="metric-label">${label}</div><div class="metric-value">${amount(value)}</div><div class="amount-exact">${escape(value)}</div></div>`).join('')}</div><p>${token.eventCount.toLocaleString()} retained transfer events for this contract.</p>` : '<p>Accounting for this contract is not yet established.</p>'}
    <div class="notice"><strong>This is token supply, not a confirmed amount owed to customers.</strong>Created tokens may include conversions. Older and dated tokens cannot simply be added to this supply. The snapshot is historical, not a live balance.</div>
    <details class="reading-notes"><summary>Other Fusion tokens and accounting limits</summary><p>Each row is a separate contract. Dated tokens represent rights over different periods; migrations may overlap older and newer balances. Do not add rows into a customer total.</p>
    <div class="table-wrap"><table><thead><tr><th>Fusion token</th><th class="numeric">Created (${escape(study.title)})</th><th class="numeric">Burned (${escape(study.title)})</th><th class="numeric">Remaining tokens</th></tr></thead><tbody>${stats.tokens.filter(row=>row.address!==selected.address).map(row=>`<tr><td><strong>${row.fullTime ? 'Other full-time token' : 'Dated token'}</strong>${explorerLink('Fusion',row.address,short(row.address))}</td><td class="numeric">${amount(rawUnits(row.mintedRaw,row.decimals))}</td><td class="numeric">${amount(rawUnits(row.burnedRaw,row.decimals))}</td><td class="numeric">${amount(remaining(row))}</td></tr>`).join('')}</tbody></table></div>
    ${stats.addressOne ? `<p>The older-token total includes <strong>${amount(rawUnits(stats.addressOne.rawAmount,stats.addressOne.decimals))} ${escape(study.title)}</strong> at ${explorerLink('Fusion',stats.addressOne.address,'address one')}. This is not a zero-address burn. Its meaning for customer claims remains unresolved.</p>` : ''}
    ${(stats.notes ?? []).map(note=>`<p>${escape(note)}</p>`).join('')}</details></section>
    <section class="panel"><h2>What has been checked</h2>${stats.checks.length ? stats.checks.map(check=>`<p>${escape(check.text)} <a href="${escape(check.source)}">Evidence ↗</a></p>`).join('') : '<p>No additional checks documented yet.</p>'}</section>`;
}

function renderVaultComparison(study) {
  const comparison = study.vaultComparison;
  const stats = study.fusionStatistics;
  const token = stats?.tokens.find(row=>row.address===selected.address);
  const groups = comparison?.groups ?? [];
  const decimals = Math.max(token?.decimals ?? 0,...groups.map(g=>g.decimals));
  const units = value => rawUnits(value.toString(),decimals).replace(/(\.\d*?)0+$/,'$1').replace(/\.$/,'');
  const total = group => group.positions.some(p=>p.rawAmount===null) || !group.positions.length ? null : group.positions.reduce((sum,p)=>sum+BigInt(p.rawAmount)*10n**BigInt(decimals-group.decimals),0n);
  const included = groups.filter(g=>g.treatment==='included');
  const separate = groups.filter(g=>g.treatment==='separate');
  const aligned = comparison && stats && Date.parse(comparison.referenceTime)===Date.parse(stats.referenceTime);
  const found = included.length && included.every(g=>total(g)!==null) ? included.reduce((sum,g)=>sum+total(g),0n) : null;
  const supply = token ? (BigInt(token.mintedRaw)-BigInt(token.burnedRaw))*10n**BigInt(decimals-token.decimals) : null;
  const gap = aligned && supply!==null && found!==null ? supply-found : null;
  const metrics = [['Fusion supply',supply,'Selected Fusion contract only'],['Found in vaults',found,separate.length?'Identified likely vaults · excludes balances shown separately':'Identified likely vaults · checked positions only'],['Comparison gap',gap,'Supply minus included vault balances']];
  return `<section class="panel"><div class="section-title"><h2>Fusion supply vs vault balances</h2>${comparison ? `<a href="${escape(comparison.source)}">Evidence and scope ↗</a>` : ''}</div>
    <p class="muted">${aligned ? `Historical snapshot · ${escape(comparison.referenceTime.replace('T',' ').replace('.000Z',' UTC'))}. Vault balances use chain blocks aligned to this time.` : 'A comparison at the same reference time is not yet available. No gap is calculated.'}</p>
    <div class="metrics">${metrics.map(([label,value,note])=>`<div class="metric"><div class="metric-label">${label} · ${escape(study.title)}</div><div class="metric-value${value===null?' unknown':''}">${value===null?'Not established':amount(units(value))}</div><p>${note}</p></div>`).join('')}</div>
    ${separate.map(g=>`<p><strong>${escape(g.chain)} ${escape(g.tokenLabel ?? study.title)}, shown separately: ${total(g)===null?'Unknown':amount(units(total(g)))} labelled token units.</strong> ${escape(g.reason)} Excluded from “Found in vaults” and the comparison gap. ${explorerLink(g.chain,g.token,'Token ↗')}</p>`).join('')}
    <p class="footnote">The gap is not a confirmed loss. This compares token units, including bridged tokens, rather than verified redeemable dollars. Other vaults may exist, and Fusion supply is not a confirmed customer claim total. Unknown balances are never counted as zero.</p>
    ${groups.length ? `<details class="reading-notes"><summary>Vault balance breakdown</summary><p>Exact amounts are available on hover. Expand a row to see every checked address, including zero and unknown balances.</p><div class="table-wrap"><table><thead><tr><th>Chain / token</th><th>Cutoff block / method</th><th class="numeric">Balance (token units)</th></tr></thead><tbody>${groups.map(g=>`<tr><td><strong>${escape(g.chain)} · ${escape(g.tokenLabel ?? study.title)}</strong>${explorerLink(g.chain,g.token,short(g.token))}${g.treatment==='separate'?'<p>Shown separately</p>':''}<details><summary>${g.positions.length} vault addresses</summary>${g.positions.map(p=>`<p>${explorerLink(g.chain,p.holder,short(p.holder))}: ${p.rawAmount===null?'Unknown':amount(rawUnits(p.rawAmount,g.decimals))}${p.source?` · <a href="${repo}${escape(p.source)}">Evidence ↗</a>`:''}</p>`).join('')}</details></td><td>${g.block.toLocaleString()}<br>${escape(g.method)}</td><td class="numeric">${total(g)===null?'Unknown':amount(units(total(g)))}</td></tr>`).join('')}</tbody></table></div></details>` : '<p>No aligned vault balance checks have been published.</p>'}</section>`;
}

function renderClaimReview(study) {
  const review = study.claimReview;
  if (!review) return '';
  if (review.referenceBlock !== study.fusionStatistics?.referenceBlock) return '<section class="panel"><h2>Outstanding claims</h2><p>Holder review does not match the supply snapshot. Consolidated claims are not established.</p></section>';
  const one = '0x0000000000000000000000000000000000000001';
  const tokens = [...new Set(review.positions.map(p=>p.token))];
  const statuses = {'expired-interval':'Expired period','future-interval':'Future period','remaining-perpetual-interval':'Unexpired rights','active-finite-interval':'Active finite period'};
  return `<section class="panel"><div class="section-title"><h2>Outstanding claims: not established</h2><a href="${escape(review.source)}">Read the holder review ↗</a></div><p>These are checked token balances, not confirmed amounts owed to customers. Address-one holdings, expired rights and possible migration overlaps remain distinct.</p><details class="reading-notes"><summary>Inspect ${review.positions.length.toLocaleString()} holder positions</summary><div class="table-wrap"><table><thead><tr><th>Fusion contract / period</th><th class="numeric">At address one</th><th class="numeric">At other addresses</th><th class="numeric">Of those, blocked</th></tr></thead><tbody>${tokens.map(token=>{
    const positions=review.positions.filter(p=>p.token===token);
    const total=predicate=>positions.filter(predicate).reduce((sum,p)=>sum+BigInt(p.rawAmount),0n).toString();
    const metadata=study.fusionStatistics.tokens.find(t=>t.address===token);
    return `<tr><td><strong>${token===selected.address?'Selected token':metadata?.fullTime?'Legacy full-time token':'Dated token'} · ${escape(statuses[positions[0].intervalStatus]??'Period not assessed')}</strong>${explorerLink('Fusion',token,short(token))}<details><summary>${positions.length} positive positions</summary>${positions.slice().sort((a,b)=>compareInteger(BigInt(b.rawAmount),BigInt(a.rawAmount))).map(p=>`<p>${explorerLink('Fusion',p.holder,short(p.holder))}: ${amount(rawUnits(p.rawAmount,p.decimals))} · <a href="${repo}${escape(p.balanceSource)}">Balance evidence ↗</a>${p.blocked===undefined?'':` · <a href="${repo}${escape(p.restrictionSource)}">${p.blocked?'Blocked':'Not blocklisted'} ↗</a>`}</p>`).join('')}</details></td><td class="numeric">${amount(rawUnits(total(p=>p.holder===one),positions[0].decimals))}</td><td class="numeric">${amount(rawUnits(total(p=>p.holder!==one),positions[0].decimals))}</td><td class="numeric">${positions.every(p=>typeof p.blocked==='boolean')?amount(rawUnits(total(p=>p.holder!==one&&p.blocked),positions[0].decimals)):'Not checked'}</td></tr>`;
  }).join('')}</tbody></table></div><p>Amounts are ${escape(study.title)} token units. Blocked is a subset of “other addresses”, not an extra balance or a deduction. Blocking can be reversible. Do not add rows across contracts or treat “other addresses” as customer-owned balances.</p>${review.notes.map(note=>`<p>${escape(note)}</p>`).join('')}</details></section>`;
}

function announcementEntries(study) {
  return (study.announcementIds ?? []).map(id=>registry.timeline?.entries.find(entry=>entry.id===id)).filter(Boolean);
}

function announcementTiming(value, publishedUtc, dateOnly = false) {
  const point=Date.parse(publishedUtc);
  if (!value || !Number.isFinite(point)) return 'Unknown date';
  if (!dateOnly) {
    const instant=Date.parse(value);
    return !Number.isFinite(instant)?'Unknown date':instant<point?'Before publication':instant>point?'After publication':'At publication time';
  }
  const dates=[...String(value).matchAll(/\d{4}-\d{2}-\d{2}/g)].map(match=>match[0]).sort();
  if (!dates.length) return 'Unknown date';
  const start=Date.parse(dates[0]+'T00:00:00Z'),end=Date.parse(dates.at(-1)+'T00:00:00Z')+86400000;
  if (!Number.isFinite(start)||!Number.isFinite(end)) return 'Unknown date';
  return end<=point?'Before publication':start>point?'After publication':'Date range overlaps publication';
}

function renderAnnouncementContext(study) {
  const entries=announcementEntries(study);
  if (!entries.length) return '';
  const outflows=study.automatedMovements?.cases.filter(c=>c.review.direction==='outgoing') ?? [];
  return `<details class="panel"><summary>Transfers and Chainge announcements</summary><p>${escape(study.announcementScopeNote)}</p><p>Counts cover retained outgoing transfer events, including small transfers and possible redemptions. They are not a count of customers or unexplained payments.</p>${entries.map(entry=>{
    const counts=new Map();for(const c of outflows){const label=announcementTiming(c.timestamp,entry.publishedUtc);counts.set(label,(counts.get(label)??0)+1);}
    return `<h3>${escape(entry.title)}</h3><p>${escape(entry.publishedUtc)} · ${entry.sourceIds.map(id=>`<a href="${escape(registry.timeline.sources[id].url)}">Statement ↗</a>`).join(' · ')}</p><p>${escape(entry.summary)}</p><p>${outflows.length?['Before publication','After publication','At publication time','Unknown date'].map(label=>`${label}: ${(counts.get(label)??0).toLocaleString()}`).join(' · '):'No retained outflow ledger is connected.'}</p><p class="footnote">${escape(entry.limitation)}</p>`;
  }).join('')}<p>Timing compares publication timestamps, not verified incident onset or the cause of a transfer. The statements are attributed claims; retained text may include later edits. <a href="#timeline">Full timeline and edit details →</a></p></details>`;
}

function renderAnnouncementTiming(value, dateOnly = false) {
  const study=registry.studies[selected.address];
  const entries=study?announcementEntries(study):[];
  if (!entries.length) return '';
  const beyondSnapshot = !dateOnly && Date.parse(value)>Date.parse(study.fusionStatistics?.referenceTime);
  return `<details class="reading-notes"><summary>Timing against announcements</summary>${beyondSnapshot?'<p><strong>After the Fusion snapshot.</strong> This review cannot establish whether a corresponding burn occurred beyond the retained Fusion cutoff.</p>':''}${entries.map(entry=>`<p><strong>${escape(announcementTiming(value,entry.publishedUtc,dateOnly))}</strong> · ${escape(entry.title)} (${escape(entry.publishedUtc)}) · <a href="${escape(registry.timeline.sources[entry.sourceIds[0]].url)}">Statement ↗</a></p>`).join('')}<p>Publication is a reference point, not a verified start of asset problems. Edited text and grouped case dates limit precision. <a href="#timeline">Read the full context →</a></p></details>`;
}

function renderVaults(study) {
  return `<div class="section-title"><h2>Vault addresses</h2><a href="${study.assessment}">Read the report ↗</a></div><div class="notice"><strong>These are likely vaults. The list may be incomplete.</strong>Each row shows one token on one chain. A vault address alone does not prove the assets were held for customers.</div><section class="panel table-wrap"><table><thead><tr><th>Chain / token</th><th>Vault address</th><th class="numeric">Balance (tokens)</th><th>Source / date</th></tr></thead><tbody>${study.vaults.map(v=>`<tr><td><strong>${escape(v.chain)}</strong>${escape(v.token)}</td><td class="address-cell"><strong>${escape(v.role)}</strong><code>${explorerLink(v.chain, v.address)}</code></td><td class="numeric"><strong>${v.balance == null ? 'Unknown' : v.balance === '0' ? '0' : amount(v.balance)}</strong>${v.balance != null && v.balance !== '0' ? `<div class="amount-exact">${v.balance}</div>` : ''}</td><td class="vault-note">${escape(v.cutoff)}<details class="reading-notes"><summary>Evidence and limits</summary><p>${escape(v.note)}</p><a class="evidence-link" href="${v.source}">View evidence ↗</a></details></td></tr>`).join('')}</tbody></table></section><p class="muted">A zero balance is an observation at a stated cutoff. An unknown balance would be shown as unknown. Small nonzero balances retain their exact value.</p>`;
}

function renderMovements(study) {
  const switcher = study.automatedMovements ? `<div class="filters"><div class="filter-field"><label for="movement-view">View</label><select id="movement-view"><option value="terminal" ${movementView === 'terminal' ? 'selected' : ''}>Latest vault transfers</option><option value="ledger" ${movementView === 'ledger' ? 'selected' : ''}>All transfers · ${study.automatedMovements.cases.length} transfers</option><option value="reviewed" ${movementView === 'reviewed' ? 'selected' : ''}>Reviewed cases · ${study.flows.length}</option></select></div></div>` : '';
  return switcher + renderAnnouncementContext(study) + (study.automatedMovements && movementView !== 'ledger' ? `<p class="muted">${study.automatedMovements.cases.length.toLocaleString()} transfers recorded. <button class="reset-filters" data-show-all-movements="true">Show all recorded transfers</button></p>` : '') + (study.automatedMovements && movementView === 'terminal' ? renderTerminalReview(study.automatedMovements) : study.automatedMovements && movementView === 'ledger' ? renderMovementLedger(study.automatedMovements) : renderReviewedMovements(study));
}

function renderReviewedMovements(study) {
  if (!['all','hide-redemptions'].includes(flowFilter) && !study.flows.some(flow => flow.status === flowFilter)) flowFilter = 'hide-redemptions';
  const flows = study.flows.filter(flow => flowFilter === 'all' || (flowFilter === 'hide-redemptions' ? flow.status !== 'Possible redemption' : flow.status === flowFilter))
    .filter(flow => tokenUnits(flow.amount) >= tokenUnits(flowMinimum))
    .sort(compareFlows);
  const current = flows.find(flow => flow.id === flowId) || flows[0];
  if (current) flowId = current.id;
  return `<div class="section-title"><h2>Reviewed cases</h2><a href="${escape(study.assessment)}">Full investigation ↗</a></div><p class="muted">${flows.length} of ${study.flows.length} cases shown · ${study.flows.length - flows.length} hidden by filters. Amounts are tokens, not a loss total.</p><details class="reading-notes"><summary>How to read this list</summary><p>Each card starts with a payment from a likely vault. Later transfers stay inside that case. Possible redemptions are hidden by default. Unexplained means no confirmed Fusion burn link; it does not prove a loss.</p><p>The default order puts unexplained cases first, then newest and largest. Grouped cases use their latest date.</p></details><div class="filters"><div class="filter-field"><label for="flow-filter">Status</label><select id="flow-filter">${['hide-redemptions','all',...new Set(study.flows.map(flow=>flow.status))].map(value=>`<option value="${escape(value)}" ${flowFilter === value ? 'selected' : ''}>${value === 'hide-redemptions' ? 'Hide possible redemptions' : value === 'all' ? 'All reviewed cases' : escape(statusLabel(value))}</option>`).join('')}</select></div><div class="filter-field"><label for="flow-sort">Sort</label><select id="flow-sort">${[['review','Unexplained first · newest'],['newest','Newest first'],['largest','Largest first']].map(([value,label])=>`<option value="${value}" ${flowSort === value ? 'selected' : ''}>${label}</option>`).join('')}</select></div><div class="filter-field"><label for="flow-minimum">Minimum amount</label><select id="flow-minimum">${['0','100','1000','10000','100000','1000000'].map(value=>`<option value="${value}" ${flowMinimum === value ? 'selected' : ''}>${value === '0' ? 'No minimum' : format(value)}</option>`).join('')}</select></div><button class="reset-filters" data-reset-flows="true">Reset filters</button></div><div class="flow-layout"><div>${flows.map(flow=>`<button class="flow-choice" data-flow="${flow.id}" aria-pressed="${current?.id === flow.id}"><span>${directionBadge(reviewedDirection(flow))}<strong>${escape(flow.title)}</strong><small>${escape(flow.date)} · ${escape(flow.chain)}</small><p class="flow-description">Last known destination: ${escape(flow.endpoint)}</p><span class="status ${flow.status === 'Unmatched' ? '' : 'neutral'}">${escape(statusLabel(flow.status))}</span></span><span class="flow-value">${amount(flow.amount)}<small style="display:block">${escape(study.title)}</small></span></button>`).join('')}</div>${current ? renderTrace(current, study.title) : '<div class="empty">No cases match this filter.</div>'}</div>`;
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

function directionLabel(direction) {
  return {incoming:'↓ Into vault',outgoing:'↑ Out of vault',internal:'↔ Between vaults'}[direction] ?? 'Direction not established';
}

function directionBadge(direction) {
  return `<span class="transfer-direction">${escape(directionLabel(direction))}</span>`;
}

function reviewedDirection(flow) {
  const steps = flow.steps.filter(step => step.chain !== 'Fusion');
  const source = steps[0];
  const recipient = steps[1];
  const vaults = registry.studies[selected.address].vaults;
  return source && recipient && source.chain === recipient.chain && vaults.some(vault => vault.chain === recipient.chain && vault.address.toLowerCase() === recipient.address.toLowerCase()) ? 'internal' : 'outgoing';
}

function traceTransferDetails(step) {
  if (!step.transaction) return '<span class="transfer-direction">Starting address</span>';
  if (step.chain === 'Fusion') return '<span class="transfer-direction">Fusion token event · not a vault transfer</span>';
  const cases = registry.studies[selected.address].automatedMovements?.cases ?? [];
  const matches = cases.filter(c => c.chain === step.chain && step.transaction.endsWith('/' + c.transaction) && (c.from === step.address.toLowerCase() || c.to === step.address.toLowerCase()) && tokenUnits(rawUnits(c.rawAmount,c.decimals)) === tokenUnits(String(step.amount)));
  if (matches.length !== 1) return '<span class="transfer-direction">Transfer details in linked transaction</span>';
  const transfer = matches[0];
  return `${directionBadge(transfer.review.direction)}<p class="transfer-endpoints"><strong>From:</strong> ${explorerLink(step.chain,transfer.from)}<br><strong>To:</strong> ${explorerLink(step.chain,transfer.to)}</p>`;
}

function renderTraceSteps(steps, asset) {
  return `<ol class="trace-path">${steps.map((step,index)=>`<li class="${steps[index+1]?.kind === 'inferred' ? 'inferred' : ''}"><span class="chain">${escape(step.chain)}${step.kind === 'inferred' ? ' · possible link' : ''}</span>${traceTransferDetails(step)}<strong>${escape(step.label)}</strong><code>${explorerLink(step.chain, step.address)}</code><div class="amount-exact">${step.amount} ${step.chain === 'Fusion' ? 'Fusion ' : ''}${escape(step.asset ?? asset)}</div><time>${escape(step.time)}</time>${step.note ? `<p>${escape(step.note)}</p>` : ''}${step.transaction ? `<a href="${step.transaction}">View transaction ↗</a>` : ''}</li>`).join('')}</ol>`;
}

function renderTrace(flow, asset) {
  return `<section class="trace-panel" aria-label="Selected transfer path" tabindex="-1"><button class="reset-filters back-to-case" data-back-case="true">← Back to selected case</button><p class="eyebrow">Transfer details</p><h3>${escape(flow.title)}</h3>${directionBadge(reviewedDirection(flow))}<p>${escape(flow.explanation)}</p>${renderAnnouncementTiming(flow.date,true)}<p><strong>Last known destination:</strong> ${linkedText(flow.endpoint, flow.steps.at(-1)?.chain)}</p><p class="muted">Direction is relative to the likely vault. Follow the path from top to bottom. Solid line: verified transfer · Dashed line: possible link</p>${renderTraceSteps(flow.steps, asset)}${(flow.branches ?? []).map(branch => `<section class="trace-branch"><h4>${escape(branch.title)}</h4><p>${escape(branch.explanation)}</p>${renderTraceSteps(branch.steps, asset)}${branch.limitation ? `<p class="muted">${escape(branch.limitation)}</p>` : ''}${branch.source ? `<a class="evidence-link" href="${escape(branch.source)}">Evidence and later transfers ↗</a>` : ''}</section>`).join('')}<div class="trace-unknown">${flow.limitation ? escape(flow.limitation) : flow.status === 'Possible redemption' ? 'The burn/payment relationship is inferred. Later combined-balance transfers require the accompanying report.' : flow.status === 'Possible rebalancing' ? 'The exchange-internal connection is not visible on-chain.' : 'Tracing stops here. The credited exchange account, retained funds and later withdrawals are unknown.'}</div><a class="evidence-link" href="${flow.source}">Open evidence and exact transaction history ↗</a></section>`;
}

function focusTransferDetails() {
  const panel = document.querySelector('.trace-panel');
  if (!panel) return;
  panel.focus({preventScroll:true});
  panel.scrollIntoView({block:'start'});
}

search.addEventListener('input', () => { visibleCount = 50; renderCatalogue(); });
reviewedOnly.addEventListener('change', () => { visibleCount = 50; renderCatalogue(); });
more.addEventListener('click', () => { visibleCount += 50; renderCatalogue(); });
list.addEventListener('click', event => { const button = event.target.closest('[data-address]'); if (button) { tabName = 'overview'; const next = '#token=' + button.dataset.address; if (location.hash === next) { renderSelection(); } else { location.hash = next; } } });
detail.addEventListener('click', async event => {
  const button = event.target.closest('button');
  if (!button) return;
  if (button.dataset.tab) { tabName = button.dataset.tab; renderSelection(); document.querySelector(`#tab-${tabName}`).focus(); }
  if (button.dataset.showAllMovements) { ledgerAnnouncement = '';  movementView = 'ledger'; ledgerDirection = 'all'; ledgerClassification = 'all'; ledgerChain = 'all'; ledgerToken = 'all'; ledgerDestination = ''; flowMinimum = '0'; ledgerCount = 50; ledgerId = ''; renderSelection(); document.querySelector('#movement-view').focus(); }
  if (button.dataset.flow) { flowId = button.dataset.flow; renderSelection(); focusTransferDetails(); }
  if (button.dataset.openFlow) { movementView = 'reviewed'; flowId = button.dataset.openFlow; flowFilter = 'all'; flowMinimum = '0'; tabName = 'movements'; renderSelection(); focusTransferDetails(); }
  if (button.dataset.ledgerId) { ledgerId = button.dataset.ledgerId; renderSelection(); focusTransferDetails(); }
  if (button.dataset.ledgerMore) { ledgerCount += 50; renderSelection(); document.querySelector('[data-ledger-more]')?.focus(); }
  if (button.dataset.destination) { ledgerDestination = button.dataset.destination; ledgerChain = button.dataset.chain; ledgerToken = button.dataset.token; ledgerDirection = 'outgoing'; ledgerClassification = 'all'; flowMinimum = '0'; ledgerCount = 50; ledgerId = ''; renderSelection(); document.querySelector('#ledger-destination').focus(); }
  if (button.dataset.resetLedger) { ledgerAnnouncement = '';  ledgerDirection = 'outgoing'; ledgerClassification = 'hide-redemptions'; ledgerChain = 'all'; ledgerToken = 'all'; ledgerDestination = ''; flowMinimum = '0'; flowSort = 'review'; ledgerCount = 50; ledgerId = ''; renderSelection(); document.querySelector('#ledger-classification').focus(); }
  if (button.dataset.resetFlows) { flowFilter = 'hide-redemptions'; flowSort = 'review'; flowMinimum = '0'; flowId = ''; renderSelection(); document.querySelector('#flow-filter').focus(); }
  if (button.dataset.backCase) {
    const card = document.querySelector('[data-flow][aria-pressed="true"], [data-ledger-id][aria-pressed="true"]');
    card?.focus({preventScroll:true});
    card?.scrollIntoView({block:'nearest'});
  }
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
    if (id === 'ledger-announcement') ledgerAnnouncement = event.target.value;
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
function statusLabel(value) {
  return value === 'Unmatched' ? 'Unexplained' : value === 'Possible operating transition' ? 'Possible vault change' : value;
}

function classificationLabel(value) {
  const labels = {'unmatched':'No automatic match','possible-redemption':'Possible redemption','possible-deposit':'Possible deposit','operating-address-transfer':'Between vaults','reported-asset-exchange':'Swap recorded','possible-asset-exchange':'Possible swap','possible-source-chain-transfer':'Possible transfer between chains','zero-value':'Zero amount'};
  return labels[value] ?? value.replaceAll('-', ' ');
}
function depletionLabel(value) { return value === null ? 'Unavailable' : (value / 100).toFixed(2) + '%'; }

function ledgerSelect(id, label, options, selectedValue) {
  return `<div class="filter-field"><label for="${id}">${label}</label><select id="${id}">${options.map(([value,text]) => `<option value="${escape(value)}" ${selectedValue === value ? 'selected' : ''}>${escape(text)}</option>`).join('')}</select></div>`;
}

function renderMovementLedger(report) {
  const destinations = new Map(report.destinations.map(d => [destinationKey(d.chain, d.token, d.address), d]));
  const minimum = comparableRaw(flowMinimum, 0);
  const announcements = announcementEntries(registry.studies[selected.address] ?? {});
  const [timing, announcementId] = ledgerAnnouncement.split(':');
  const announcement = announcements.find(entry=>entry.id===announcementId);
  if (!announcement) ledgerAnnouncement = '';
  const matches = report.cases.filter(c => (ledgerDirection === 'all' || c.review.direction === ledgerDirection)
    && (ledgerClassification === 'all' || (ledgerClassification === 'hide-redemptions' ? c.review.classification !== 'possible-redemption' : c.review.classification === ledgerClassification))
    && (ledgerChain === 'all' || c.chain === ledgerChain)
    && (ledgerToken === 'all' || c.token === ledgerToken)
    && (!announcement || announcementTiming(c.timestamp,announcement.publishedUtc) === timing)
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
  return `<div class="section-title"><h2>All transfers</h2><span class="status neutral">Automatic matches</span></div>
    <p class="muted">Showing ${visible.length} of ${matches.length.toLocaleString()} matching transfers · ${report.cases.length.toLocaleString()} recorded in total.</p><details class="reading-notes"><summary>About automatic matches</summary><p>These labels come from the matching script and may differ from reviewed cases. “No automatic match” means the script found no match under its rules. Possible redemptions are hidden by default. The saved history may be incomplete.</p></details>
    <div class="filters ledger-filters">
    ${announcements.length ? ledgerSelect('ledger-announcement','Announcement timing',[['','Any date'],...announcements.flatMap(entry=>['Before publication','After publication'].map(timing=>[timing+':'+entry.id,timing+' · '+entry.title]))],ledgerAnnouncement) : ''}
    ${ledgerSelect('ledger-direction','Direction',[['outgoing','Out of vaults'],['incoming','Into vaults'],['internal','Between vaults'],['all','All transfers']],ledgerDirection)}
    ${ledgerSelect('ledger-classification','Status',[['hide-redemptions','Hide possible redemptions'],['all','All results'],...classifications.map(c=>[c,classificationLabel(c)])],ledgerClassification)}
    ${ledgerSelect('ledger-chain','Chain',[['all','All chains'],...[...new Set(report.cases.map(c=>c.chain))].sort().map(c=>[c,c])],ledgerChain)}
    ${ledgerSelect('ledger-token','Token contract',[['all','All token contracts'],...[...new Set(report.cases.filter(c=>ledgerChain === 'all' || c.chain === ledgerChain).map(c=>c.token))].sort().map(c=>[c,c])],ledgerToken)}
    ${ledgerSelect('ledger-sort','Sort',[['review','No match first · newest'],['newest','Newest first'],['largest','Largest first'],['depletion','Balance reduction first'],['destination','Recipient total without a match']],flowSort)}
    ${ledgerSelect('ledger-minimum','Minimum amount',['0','100','1000','10000','100000','1000000'].map(v=>[v,v === '0' ? 'No minimum' : format(v)]),flowMinimum)}
    <div class="filter-field"><label for="ledger-destination">Recipient address</label><input id="ledger-destination" value="${escape(ledgerDestination)}" placeholder="Full address or fragment" spellcheck="false"></div><button class="reset-filters" data-reset-ledger="true">Reset filters</button><button class="reset-filters" data-show-all-movements="true">Show all recorded transfers</button></div>
    <details class="reading-notes"><summary>Amounts and balance estimates</summary><p>Amounts are token units, not current USD values. Balance reduction = payment / reconstructed sender balance before payment, assuming a zero opening balance. It is conditional on history coverage; impossible ratios are unavailable. Sorting helps prioritize review; it does not establish wrongdoing.</p></details>
    ${renderDestinations(report.destinations)}
    <div class="flow-layout"><div>${visible.map(c=>`<div class="movement-row"><button class="flow-choice" data-ledger-id="${escape(c.id)}" aria-pressed="${current?.id === c.id}"><span>${directionBadge(c.review.direction)}<strong>${escape(classificationLabel(c.review.classification))}</strong><small>${escape(c.timestamp ?? 'Unknown date')} · ${escape(c.chain)}</small><p class="flow-description">From: ${escape(short(c.from))}<br>To: ${escape(short(c.to))}</p><small>Estimated balance change: ${depletionLabel(c.depletionBasisPoints)}</small>${c.review.flags.length ? `<p class="flow-description">${c.review.flags.map(f=>escape(classificationLabel(f))).join(' · ')}</p>` : ''}</span><span class="flow-value">${amount(rawUnits(c.rawAmount,c.decimals))}<small style="display:block">${escape(report.asset)}</small></span></button><p class="movement-explorers">${explorerLink(c.chain, c.from, "Sender ↗")} · ${explorerLink(c.chain, c.to, "Recipient ↗")} · ${explorerLink(c.chain, c.transaction, "Transaction ↗")}</p></div>`).join('')}${matches.length > visible.length ? '<button class="load-more" data-ledger-more="true">Show 50 more transfers</button>' : ''}</div>${current ? renderLedgerDetail(current,destinations.get(destinationKey(current.chain,current.token,current.to)),report) : '<div class="empty">No transfers match these filters. Reset filters to restore the default view.</div>'}</div>`;
}

function renderDestinations(destinations) {
  const ranked = destinations.filter(d => ledgerChain === 'all' || d.chain === ledgerChain).slice().sort((a,b)=>compareInteger(comparableRaw(a.unmatchedOutflowRaw,a.decimals),comparableRaw(b.unmatchedOutflowRaw,b.decimals)) || b.count-a.count).slice(0,10);
  return `<details class="panel destination-panel"><summary>Recipients with the most transfers without an automatic match</summary><p>Gross transfers across all retained outflows, grouped separately by chain, token and recipient. Includes small transfers regardless of the amount filter. Only the chain filter applies here. No netting against returns; these totals do not establish ownership, unique capital or losses. Click a recipient to show its outflows.</p><div class="table-wrap"><table><thead><tr><th>Chain / token / recipient</th><th>Outflow count</th><th class="numeric">Gross (token units)</th><th class="numeric">No match (tokens)</th></tr></thead><tbody>${ranked.map(d=>`<tr><td>${escape(d.chain)}<code class="address-text">Token: ${explorerLink(d.chain, d.token)}</code><button class="copy" data-destination="${escape(d.address)}" data-chain="${escape(d.chain)}" data-token="${escape(d.token)}">Filter recipient</button> ${explorerLink(d.chain, d.address)}</td><td>${d.count} (${d.unmatchedCount} without a match)</td><td class="numeric">${amount(rawUnits(d.grossOutflowRaw,d.decimals))}</td><td class="numeric">${amount(rawUnits(d.unmatchedOutflowRaw,d.decimals))}</td></tr>`).join('')}</tbody></table></div></details>`;
}

function renderLedgerDetail(c, destination, report) {
  const delayed = report.delayedReview?.find(m => m.sourceEventId === c.id);
  const traced = registry.studies[selected.address].flows.filter(flow => flow.steps.some(step => step.transaction?.endsWith('/' + c.transaction)));
  return `<section class="trace-panel ledger-detail" aria-label="Selected transfer" tabindex="-1">${movementView === 'ledger' ? '<button class="reset-filters back-to-case" data-back-case="true">← Back to selected transfer</button>' : ''}<p class="eyebrow">Automatic match · needs review</p><h3>${escape(classificationLabel(c.review.classification))}</h3>${directionBadge(c.review.direction)}<p>One source-chain transfer event. The recipient shown is the immediate recipient, not a traced final destination.</p><code class="address-text">${eventLink(c.id)}</code><p>${escape(c.timestamp ?? 'Unknown date')} · ${escape(c.chain)}</p><p><strong>${amount(rawUnits(c.rawAmount,c.decimals))} ${escape(report.asset)}</strong></p><code class="address-text">Token: ${explorerLink(c.chain, c.token)}<br>From: ${explorerLink(c.chain, c.from)}<br>To: ${explorerLink(c.chain, c.to)}</code>${explorerLink(c.chain, c.transaction, "View source transaction ↗")}<p>Receipt verification: ${c.receiptVerified ? 'verified' : 'not verified'}.</p><h3>Estimated vault balance</h3><p>${c.review.direction === 'outgoing' ? `Before: ${c.review.reconstructedBalanceBeforeRaw === null ? 'Unavailable' : amount(rawUnits(c.review.reconstructedBalanceBeforeRaw,c.decimals))}<br>After: ${c.review.reconstructedBalanceAfterRaw === null ? 'Unavailable' : amount(rawUnits(c.review.reconstructedBalanceAfterRaw,c.decimals))}<br>Reduction: ${depletionLabel(c.depletionBasisPoints)}. Conditional on retained history and zero opening balance.` : 'Balance-depletion comparison applies to outflows only.'}</p>
    ${destination ? `<h3>Same recipient · ${escape(c.chain)}</h3><p>Across the complete retained outflow history for this token:<br>${destination.count} payments · ${amount(rawUnits(destination.grossOutflowRaw,destination.decimals))} gross token units.<br>${destination.unmatchedCount} without a match · ${amount(rawUnits(destination.unmatchedOutflowRaw,destination.decimals))} token units.<br>Includes payments hidden by filters. Returns are not deducted.</p>` : ''}
    ${traced.length ? `<h3>Related traced cases</h3><p>These reports may contain explanations beyond the automated matching rules.</p>${traced.map(flow=>`<button class="reset-filters" data-open-flow="${escape(flow.id)}">${escape(flow.title)} →</button>`).join('')}` : ''}
    ${renderAnnouncementTiming(c.timestamp)}<h3>Possible matches</h3><p>${c.review.candidates.length} Fusion candidates · ${c.review.sourceCandidates.length} source-chain candidates. Candidates are hypotheses; competing candidates are not independently settled payments.</p>
    ${c.review.candidates.map(p=>`<div class="candidate"><code class="address-text">${eventLink(p.fusionEventId)}</code><p>${escape(p.rule)} · delay ${escape(p.delay)} · fee hypothesis ${escape(p.feeRawHypothesis)} raw units · ${p.competing ? 'competing' : 'no competing normal candidate'}</p></div>`).join('')}
    ${c.review.sourceCandidates.map(p=>`<div class="candidate"><code class="address-text">${eventLink(p.otherEventId)}</code><p>Delay ${escape(p.delay)} · fee ${escape(p.feeRawHypothesis)} raw units · ${p.competing ? 'competing' : 'no competing normal candidate'}</p></div>`).join('')}
    <h3>Possible matches over a longer period</h3><p>${delayed?.candidates.length ?? 0} Fusion candidates · ${delayed?.sourceCandidates.length ?? 0} source-chain candidates. Wider-window hypotheses are kept separate and do not change the normal classification.</p>${delayed ? `<pre class="json-evidence">${linkedEvidence({fusion:delayed.candidates,sourceChain:delayed.sourceCandidates}, c.chain)}</pre>` : ''}
    <details><summary>Swap evidence (${c.review.exchangeEvidence.length})</summary><pre class="json-evidence">${linkedEvidence(c.review.exchangeEvidence, c.chain)}</pre></details>
    <h3>Saved evidence</h3>${c.evidence.map(path=>`<a class="evidence-link address-text" href="${repo}${path.split('/').map(encodeURIComponent).join('/')}">${escape(path)} ↗</a>`).join('')}<details><summary>Data sources and limitations</summary><p>Configuration SHA-256: <code class="address-text">${escape(report.configurationSha256)}</code></p><pre class="json-evidence">${escape(JSON.stringify(report.limitations,null,2))}</pre></details></section>`;
}

function terminalPayment(id, byId, asset) {
  const c = byId.get(id);
  return c ? `${directionBadge(c.review.direction)}<strong>${amount(rawUnits(c.rawAmount,c.decimals))} ${escape(asset)}</strong><small>${escape(c.timestamp ?? 'Unknown date')}<br>${escape(classificationLabel(c.review.classification))} · ${escape(c.review.direction)}</small>` : '<strong>None observed</strong>';
}

function renderTerminalReview(report) {
  const byId = new Map(report.cases.map(c=>[c.id,c]));
  const ids = new Set((report.terminal ?? []).flatMap(s=>[s.lastSubstantialOutflowId,s.finalOutflowId]).filter(Boolean));
  const cases = [...ids].map(id=>byId.get(id)).filter(Boolean).sort((a,b)=>(b.timestamp ?? '').localeCompare(a.timestamp ?? '') || a.id.localeCompare(b.id));
  return `<div class="section-title"><h2>Latest vault transfers</h2><span class="status neutral">${cases.length} transfers · newest first</span></div>
    <p class="muted">The last large payment and last recorded payment from each vault. Open a card for details. The saved history may be incomplete.</p>
    <div class="closing-list">${cases.map(c=>renderClosingTransfer(c,report)).join('') || '<div class="empty">No closing transfers available.</div>'}</div>
    <details class="panel"><summary>How this shortlist is selected</summary><p>Large means at least ${amount(rawUnits(report.terminalPolicy.largeOutflowRaw,report.terminalPolicy.decimals))} ${escape(report.asset)}. Final means last in retained evidence, not proof that activity ended. A payment that meets both criteria appears once. Small final payments and internal transfers are included. Amounts are token units, not current dollar values. A transfer without a match is not proof of loss.</p></details>
    ${renderTerminalConvergence(report,byId)}`;
}

function renderClosingTransfer(c, report) {
  const traced = registry.studies[selected.address].flows.filter(flow=>flow.steps.some(step=>step.transaction?.endsWith('/' + c.transaction)));
  const summaries = report.terminal.filter(s=>s.finalOutflowId === c.id || s.lastSubstantialOutflowId === c.id);
  const final = summaries.some(s=>s.finalOutflowId === c.id);
  const large = summaries.some(s=>s.lastSubstantialOutflowId === c.id);
  const label = c.review.direction === 'internal' ? 'Internal transfer' : c.review.classification === 'unmatched' ? 'No automatic match' : classificationLabel(c.review.classification);
  const destination = report.destinations.find(d=>d.chain === c.chain && d.token === c.token && d.address === c.to);
  const date = c.timestamp ? new Date(c.timestamp).toLocaleDateString('en-GB',{day:'numeric',month:'short',year:'numeric',timeZone:'UTC'}) : 'Unknown date';
  return `<details class="closing-card"><summary><span class="closing-date">${escape(date)}<small>${escape(c.chain)}</small></span><span class="closing-amount">${amount(rawUnits(c.rawAmount,c.decimals))} <small>${escape(report.asset)}</small></span><span class="closing-explanation">${directionBadge(c.review.direction)}<strong>${escape(traced.length ? traced.map(f=>statusLabel(f.status)).join(' · ') : label)}</strong><small>${traced.length ? 'Traced case available' : `To ${escape(short(c.to))}`} · ${large && final ? 'Last large and last recorded payment' : large ? 'Last large payment' : 'Last recorded payment'}</small></span><span class="closing-toggle" aria-hidden="true">＋</span></summary>
    <div class="closing-body">${traced.map(flow=>`<h3>${escape(flow.title)}</h3><p>${escape(flow.explanation)}</p><p><strong>Last known destination:</strong> ${linkedText(flow.endpoint, flow.steps.at(-1)?.chain)}</p><button class="reset-filters" data-open-flow="${escape(flow.id)}">View transfer details →</button>`).join('') || `<p>${c.review.direction === 'internal' ? 'A transfer between tracked vault addresses.' : c.review.classification === 'unmatched' ? 'The script found no match under its normal rules. The purpose of this payment remains unresolved.' : 'This is a provisional automated explanation; inspect the candidate relationships below.'}</p>`}
    <p class="transfer-endpoints"><strong>From:</strong> ${explorerLink(c.chain,c.from)}<br><strong>To:</strong> ${explorerLink(c.chain,c.to)}</p>
    <details class="closing-evidence"><summary>Transaction and possible matches</summary>${renderLedgerDetail(c,destination,report)}</details>
    <details class="closing-evidence"><summary>Vault balance details</summary>${summaries.map(s=>`<p>${escape(s.chain)} · <code class="address-text">${explorerLink(s.chain, s.address)}</code></p><p>Last estimated balance: <strong>${amount(rawUnits(s.closingBalanceRaw,s.decimals))} ${escape(report.asset)}</strong>. Closing sequence reduction: ${depletionLabel(s.netReductionBasisPoints)}.</p><p>Assumes zero opening balance and depends on retained history. ${s.nonnegativeHistory ? 'No negative balance encountered.' : 'Negative balances occurred; coverage is inconsistent.'} A final zero does not establish a complete vault inventory.</p>`).join('')}</details></div></details>`;
}

function renderTerminalConvergence(report, byId) {
  return `<details class="panel"><summary>Shared recipients · ${report.sharedTerminalDestinations?.length ?? 0} same-chain groups / ${report.crossChainTerminalRecipients?.length ?? 0} cross-chain address groups</summary><p>Only recipients shared by distinct vault addresses on the same chain and token are grouped. This tests direct transfers in the selected sequences; it does not trace later hops or establish who controls an address.</p>${(report.sharedTerminalDestinations ?? []).map(d=>`<h3>${escape(d.chain)} · ${explorerLink(d.chain, d.address)}</h3><code class="address-text">Token: ${explorerLink(d.chain, d.token)}</code><p>Vault addresses: ${d.sourceAddresses.map(address=>explorerLink(d.chain, address)).join(', ')}</p>${d.eventIds.map(id=>`<p><code class="address-text">${eventLink(id)}</code>${terminalPayment(id,byId,report.asset)}</p>`).join('')}`).join('') || '<p>No shared immediate recipients found within this comparison scope. Later intermediary convergence is not ruled out.</p>'}<h3>Same recipient address across chains</h3><p>Identical address bytes are a lead, not proof of common control. Amounts and transfers stay separate by chain.</p>${(report.crossChainTerminalRecipients ?? []).map(d=>`<span>Recipient links by chain:</span>${d.eventIds.map(id=>`<p>${escape(byId.get(id).chain)} · ${explorerLink(byId.get(id).chain, byId.get(id).from, short(byId.get(id).from))} → ${explorerLink(byId.get(id).chain, d.address, short(d.address))}<br>${terminalPayment(id,byId,report.asset)}</p>`).join('')}`).join('') || '<p>No cross-chain recipient-address reuse found in these sequences.</p>'}</details>`;
}

function renderTimeline() {
  const timeline = registry.timeline;
  if (!timeline) return '<div class="empty">No timeline available.</div>';
  const coverage = timeline.coverage;
  return `<p class="eyebrow">Project history</p><div class="section-title"><h1>Service timeline</h1><span class="status neutral">${escape(timeline.status)}</span></div>
    <p>Reported disruptions, funding plans and restoration updates from the founder, community admins and former contributors. Read from oldest to newest, and expand any event to inspect the preserved statements.</p>
    <div class="notice"><strong>Statements are evidence of what was said.</strong>They do not independently prove a blacklist, funding receipt or repayment. No single shutdown date has been established.</div>
    <p class="muted">${timeline.entries.length} milestones · reviewed ${escape(coverage.reviewedDate)} · all publication times in UTC. <a href="${repo}research/service-timeline.md">Method and open questions ↗</a></p>
    ${renderCommunityReview(timeline.communityReview, timeline.sources)}
    <div class="service-timeline">${timeline.entries.map(entry=>renderTimelineEntry(entry,timeline.sources)).join('')}</div>
    <details class="panel"><summary>About the sources</summary><p>Retained export: ${escape(coverage.firstRetainedUtc)} to ${escape(coverage.lastRetainedUtc)} · ${coverage.retainedRecords.toLocaleString()} records. Export completion time was not supplied.</p><p>${escape(coverage.attribution)}</p><p>${escape(coverage.method)}</p><ul>${coverage.limitations.map(item=>`<li>${escape(item)}</li>`).join('')}</ul><p>Private original SHA-256:<code class="address-text">${escape(coverage.rawExportSha256)}</code></p><a href="${repo}registry/timeline.json">Reviewed timeline and excerpts ↗</a></details>`;
}

function renderTimelineEntry(entry, sources) {
  return `<article class="panel timeline-entry"><p class="eyebrow"><time datetime="${escape(entry.publishedUtc)}">${escape(entry.publishedUtc.slice(0,10))}</time> · ${escape(entry.scope)}</p><h2>${escape(entry.title)}</h2><p>${escape(entry.summary)}</p>
    <details><summary>Sources and open questions · ${entry.sourceIds.length} statement${entry.sourceIds.length === 1 ? '' : 's'}</summary><p><strong>Event or target time:</strong> ${escape(entry.eventTime)}</p><p><strong>Not established:</strong> ${escape(entry.limitation)}</p>
    ${entry.sourceIds.map(id=>{const source=sources[id];return `<section class="timeline-source"><p><a href="${escape(source.url)}">${escape(source.author)} · message ${source.messageId} ↗</a><br><time datetime="${escape(source.publishedUtc)}">${escape(source.publishedUtc)}</time>${source.lastEditedUtc ? `<br>Last retained edit: ${escape(source.lastEditedUtc)}` : ''}</p>${[source.excerpt,...(source.additionalExcerpts ?? [])].map(excerpt=>`<blockquote>${escape(excerpt)}</blockquote>`).join('')}<p class="footnote">${source.excerptIsCompleteMessage ? 'Complete retained message text.' : 'Selected verbatim excerpt(s); intervening or surrounding text omitted.'} Live message content has not been reverified.</p>${source.document ? `<p><a href="${repo}${escape(source.document.path)}">Preserved PDF: ${escape(source.document.title)} ↗</a> · ${source.document.pages} pages</p><p>${escape(source.document.summary)}</p><p class="footnote">Preserved PDF SHA-256:<code class="address-text">${escape(source.document.sha256)}</code></p>` : ''}</section>`;}).join('')}</details></article>`;
}

function renderCommunityReview(review, sources) {
  if (!review) return '';
  return `<details class="panel"><summary>${escape(review.title)}</summary><p class="status neutral">${escape(review.status)}</p><p>${escape(review.method)}</p>${review.periods.map(period=>`<section class="timeline-source"><h3>${escape(period.period)} · ${escape(period.scope)}</h3><p>${escape(period.reports)}</p><p><strong>Compared with the announcements:</strong> ${escape(period.comparison)}</p>${period.sourceIds.map(id=>`<a href="${escape(sources[id].url)}">Official/admin message ${sources[id].messageId} ↗</a>`).join(' · ')}</section>`).join('')}<p><strong>Privacy:</strong> ${escape(review.privacy)}</p><p><strong>Limits:</strong> ${escape(review.limitations)}</p></details>`;
}

document.querySelector('#copy-donation').addEventListener('click', async () => {
  const address = document.querySelector('#donation-address');
  const status = document.querySelector('#donation-status');
  try {
    await navigator.clipboard.writeText(address.value);
    status.textContent = 'Address copied · BNB Smart Chain (BSC)';
  } catch {
    address.focus();
    address.select();
    status.textContent = 'Select and copy the address above · BNB Smart Chain (BSC)';
  }
});

function explorerLink(chain, value, label = value) {
  const sites = { Fusion: ['https://fsnex.net', 'transaction'], BSC: ['https://bscscan.com', 'tx'], Ethereum: ['https://etherscan.io', 'tx'], Avalanche: ['https://snowtrace.io', 'tx'], Arbitrum: ['https://arbiscan.io', 'tx'], Optimism: ['https://optimistic.etherscan.io', 'tx'], Polygon: ['https://polygonscan.com', 'tx'], Base: ['https://basescan.org', 'tx'], Aurora: ['https://explorer.aurora.dev', 'tx'], Rollux: ['https://explorer.rollux.com', 'tx'] };
  const site = sites[chain];
  if (!site || !/^0x(?:[a-fA-F0-9]{40}|[a-fA-F0-9]{64})$/.test(value)) return escape(label);
  const path = value.length === 42 ? 'address' : site[1];
  return `<a href="${site[0]}/${path}/${encodeURIComponent(value)}" target="_blank" rel="noopener noreferrer" title="${escape(chain)} explorer · ${escape(value)}"${label === "↗" ? ` aria-label="View contract ${escape(value)} on ${escape(chain)} explorer"` : ""}>${escape(label)}</a>`;
}

function eventLink(value) {
  const match = /^(Fusion|BSC|Ethereum|Avalanche|Arbitrum|Optimism):(0x[a-fA-F0-9]{64}):\d+$/.exec(value);
  return match ? explorerLink(match[1], match[2], value) : escape(value);
}

function linkedText(value, chain) {
  return escape(value).replace(/(?:Fusion|BSC|Ethereum|Avalanche|Arbitrum|Optimism):0x[a-fA-F0-9]{64}:\d+|0x(?:[a-fA-F0-9]{64}|[a-fA-F0-9]{40})(?![a-fA-F0-9])/g, reference => reference.includes(':') ? eventLink(reference) : explorerLink(chain, reference));
}

function linkedEvidence(value, chain) {
  return linkedText(JSON.stringify(value, null, 2), chain);
}

const donationAddress = document.querySelector('#donation-address').value;
document.querySelector('#donation-explorer').innerHTML = explorerLink('BSC', donationAddress, 'View on BscScan ↗');
