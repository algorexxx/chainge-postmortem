const fs=require('fs'),vm=require('vm'),assert=require('assert/strict');
const html=fs.readFileSync('dist/index.html','utf8');
const payload=html.match(/<script id="registry-data" type="application\/json">([\s\S]*?)<\/script>/)[1];
const elements=new Map();
function element(key){if(!elements.has(key))elements.set(key,{textContent:key==='#registry-data'?payload:'',innerHTML:'',value:key==='#donation-address'?'0xa3ce60d2dbf51afa0ab106df1c44a2e48853817a':'',checked:false,addEventListener(){},focus(){}});return elements.get(key);}
const context=vm.createContext({document:{querySelector:element,getElementById:id=>element('#'+id)},location:{hash:''},window:{addEventListener(){}},URLSearchParams,console});
vm.runInContext(fs.readFileSync('Chainge.Postmortem.Site/Assets/app.js','utf8'),context);
const run=s=>vm.runInContext(s,context);
for(const token of ['0xfc9e57d7543e0591824121fda6461fe2148a5966','0x9d7005f9175856bda5deb153f86a11153d9d0fa7']){
 context.location.hash='#token='+token;
 for(const tab of ['overview','vaults','movements']){
  run(`tabName='${tab}';renderSelection();`);
  assert(!element('#detail').innerHTML.includes('undefined'));
  assert(element('#detail').innerHTML.includes('Transfers'));
 }
 for(const view of ['reviewed','terminal','ledger']){
  run(`movementView='${view}';renderSelection();`);
  assert(element('#detail').innerHTML.includes('View'));
 }
 run("movementView='reviewed';flowFilter='all';renderSelection();");
 assert(element('#detail').innerHTML.includes('Possible redemption'));
 run("flowFilter='hide-redemptions';renderSelection();");
 assert(!/class="flow-choice"[^]*?class="status[^>]*">Possible redemption/.test(element('#detail').innerHTML.split('class="trace-panel"')[0]));
}
context.location.hash='#timeline';run('renderSelection()');assert(element('#detail').innerHTML.includes('About the sources'));
console.log('Rendered BUSD and USDC overview, vaults, all three transfer views, redemption filters and timeline successfully.');
context.location.hash='#token=0xfc9e57d7543e0591824121fda6461fe2148a5966';
run("tabName='movements';movementView='reviewed';flowFilter='all';renderSelection();");
const reviewedHtml=element('#detail').innerHTML;
for(const card of reviewedHtml.matchAll(/<button class="flow-choice"[^]*?<\/button>/g))assert(!card[0].includes('<a '));
assert(reviewedHtml.includes('Possible vault change'));
assert(reviewedHtml.includes('Back to selected case'));
for(const control of ['flow-filter','flow-sort','flow-minimum','movement-view'])assert(reviewedHtml.includes(`<div class="filter-field"><label for="${control}">`));
const tx='0x'+'a'.repeat(64),address='0x'+'b'.repeat(40);
assert(run(`eventLink('Optimism:${tx}:0')`).includes(`https://optimistic.etherscan.io/tx/${tx}`));
assert(run(`eventLink('Arbitrum:${tx}:0')`).includes(`https://arbiscan.io/tx/${tx}`));
assert(run(`explorerLink('Optimism','${address}')`).includes(`https://optimistic.etherscan.io/address/${address}`));
assert(run(`explorerLink('Fusion','${address}','↗')`).includes('aria-label="View contract'));
console.log('Checked card/link separation, complete status options, grouped filters, back navigation markup and Optimism/Arbitrum explorer links.');
run("flowFilter='Possible rebalancing';renderSelection();");
assert.equal(run('flowFilter'),'hide-redemptions');
assert(element('#detail').innerHTML.includes('value="hide-redemptions" selected'));
console.log('Unsupported status filter resets when changing token.');
assert.equal(run("directionLabel('incoming')"),'↓ Into vault');
assert.equal(run("directionLabel('outgoing')"),'↑ Out of vault');
assert.equal(run("directionLabel('internal')"),'↔ Between vaults');
assert.equal(run("directionLabel('unknown')"),'Direction not established');
for(const [direction,label] of [['incoming','↓ Into vault'],['outgoing','↑ Out of vault'],['internal','↔ Between vaults']]){
 run(`movementView='ledger';ledgerDirection='${direction}';ledgerClassification='all';renderSelection();`);
 assert(element('#detail').innerHTML.includes(label));
 assert(element('#detail').innerHTML.includes('From:'));
 assert(element('#detail').innerHTML.includes('To:'));
}
assert(run("traceTransferDetails({chain:'Fusion',transaction:'https://fsnex.net/transaction/0x123'})").includes('not a vault transfer'));
assert(run("traceTransferDetails({chain:'Ethereum',transaction:'https://etherscan.io/tx/0x123',address:'0x'+'a'.repeat(40),amount:'1000'})").includes('Transfer details in linked transaction'));
console.log('Checked incoming, outgoing, between-vault labels, endpoint visibility and unknown/Fusion distinctions.');
const published = JSON.parse(fs.readFileSync('registry/studies.json','utf8'));
for(const [key,study] of Object.entries(published)) {
  assert.equal(study.schemaVersion,1);
  const groupKeys = new Set();
  for(const group of study.vaultComparison?.groups ?? []) {
    const groupKey = group.chain+':'+group.token;
    assert(!groupKeys.has(groupKey),'Duplicate comparison token group');groupKeys.add(groupKey);
    assert.equal(new Set(group.positions.map(p=>p.holder)).size,group.positions.length,'Duplicate vault position');
  }
  context.location.hash='#token='+key;run("tabName='overview';renderSelection();");
  const output=element('#detail').innerHTML;
  for(const label of ['Fusion supply vs vault balances','Found in vaults','Comparison gap','Investigation notes'])assert(output.includes(label));
  assert(output.includes(study.fusionStatistics ? study.title+' on Fusion' : 'No supply is assumed'));
  assert(!output.includes('undefined'));
}
context.location.hash='#token=0x9d7005f9175856bda5deb153f86a11153d9d0fa7';run("tabName='overview';renderSelection();");
for(const value of ['23.804358342991240779','1042056.616342023065865707','1,042,080.42'])assert(element('#detail').innerHTML.includes(value),value);
context.location.hash='#token=0xfc9e57d7543e0591824121fda6461fe2148a5966';run("renderSelection();");
for(const value of ['3191.727791325249263627','2,284,499.38','Comparison gap','Backing is unverified'])assert(element('#detail').innerHTML.includes(value),value);
const synthetic = {
  schemaVersion:1,title:'TEST',status:'Partial investigation',reviewed:'2026-10-15',scope:'Fixture',assessment:'https://example.org',vaults:[],flows:[],overviewLinks:[],
  fusionStatistics:{referenceBlock:1,referenceTime:'2025-01-01T00:00:00Z',source:'https://example.org',tokens:[{address:'0x1111111111111111111111111111111111111111',decimals:6,fullTime:true,mintedRaw:'10000000',burnedRaw:'0',eventCount:1}],checks:[],notes:[]},
  vaultComparison:{referenceTime:'2025-01-01T00:00:00Z',source:'https://example.org',groups:[
    {chain:'Ethereum',token:'0x2222222222222222222222222222222222222222',block:1,decimals:18,method:'Fixture',treatment:'included',positions:[{holder:'0x3333333333333333333333333333333333333333',rawAmount:'2000000000000000000'}]},
    {chain:'Base',token:'0x4444444444444444444444444444444444444444',block:1,decimals:24,method:'Fixture',treatment:'separate',reason:'Unverified fixture backing',positions:[{holder:'0x3333333333333333333333333333333333333333',rawAmount:'7000000000000000000000000'}]}
  ]}
};
context.fixture=synthetic;
run("selected={address:'0x1111111111111111111111111111111111111111'};tabName='overview';");
const render=()=>run('renderStudy(fixture)');
let result=render();
assert(result.includes('title="8"'),'10 supply minus 2 included, excluding 7 separate');
assert(result.includes('Base TEST, shown separately'));
assert(!result.includes('BUSD')&&!result.includes('USDC')&&!result.includes('Rollux'));
synthetic.vaultComparison.groups[0].positions[0].rawAmount=null;
assert(!render().includes('title="8"'));assert(render().includes('Not established'));
synthetic.vaultComparison.groups[0].positions[0].rawAmount='0';assert(render().includes('title="10"'));
synthetic.vaultComparison.referenceTime='2025-01-02T00:00:00Z';assert(render().includes('No gap is calculated'));
synthetic.vaultComparison.groups=[];assert(render().includes('No aligned vault balance checks'));
delete synthetic.fusionStatistics;delete synthetic.vaultComparison;assert(render().includes('No supply is assumed'));
for(const view of ['vaults','movements']){run(`tabName='${view}';movementView='reviewed';`);assert(!render().includes('undefined'));}
console.log('Shared layout verified for BUSD, USDC and a synthetic asset; unknown, zero, differing precision, separate balances, missing evidence and mismatched dates remain distinct.');
const cutoff='2025-05-27T13:35:10Z';
for(const [date,expected] of [['2025-05-27T13:35:09Z','Before publication'],['2025-05-27T13:35:11Z','After publication'],[cutoff,'At publication time'],[null,'Unknown date'],['invalid','Unknown date']])assert.equal(run(`announcementTiming(${JSON.stringify(date)},'${cutoff}')`),expected);
for(const [date,expected] of [['2025-05-26','Before publication'],['2025-05-28','After publication'],['2025-05-27','Date range overlaps publication'],['2025-05-01 to 2025-06-01','Date range overlaps publication']])assert.equal(run(`announcementTiming('${date}','${cutoff}',true)`),expected);
for(const [key,study] of Object.entries(published)){
 if(!study.claimReview)continue;
 context.location.hash='#token='+key;run("tabName='overview';renderSelection();");
 assert(element('#detail').innerHTML.includes(`Inspect ${study.claimReview.positions.length} holder positions`));
 assert(element('#detail').innerHTML.includes('Outstanding claims: not established'));
 for(const id of study.announcementIds){assert(run(`registry.timeline.entries.some(e=>e.id==='${id}')`));}
 run("tabName='movements';movementView='ledger';ledgerDirection='outgoing';ledgerClassification='all';ledgerChain='all';ledgerToken='all';ledgerDestination='';flowMinimum='0';ledgerAnnouncement='After publication:2025-suspension';renderSelection();");
 assert(element('#detail').innerHTML.includes(study.title==='USDC'?'Showing 3 of 3 matching transfers':'Showing 0 of 0 matching transfers'));
 if(study.title==='USDC')assert(element('#detail').innerHTML.includes('After the Fusion snapshot.'));
}
console.log('Verified holder inventory display, announcement references, exact-time boundaries, uncertain ranges and post-suspension filters.');
