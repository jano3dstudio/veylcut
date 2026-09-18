const assert=require('node:assert/strict');
const {plan,locate,safeRect}=require('../ui/edit-core.js');
for(const source of [.4,2,12,90,3600])for(const target of [6,15,60,300])for(const prompt of ['schneller Gameplay-Teaser','ruhiger Showcase','Cinematic','uncut']){
 const p=plan(source,target,prompt);assert(p.duration<=source);assert(p.segments.length<=120);
 assert(Math.abs(p.segments.reduce((n,s)=>n+s.duration,0)-p.duration)<1e-6);
 for(const s of p.segments){assert(s.start>=0);assert(s.duration>0);assert(s.start+s.duration<=source+1e-6);}
 const last=locate(p.segments,p.duration);assert(last.source<=source+1e-6);
}
assert(plan(60,15,'action').segments.length>plan(60,15,'ruhiger Showcase').segments.length);
assert.equal(plan(60,15,'uncut').segments.length,1);
const r=safeRect(1080,1920,[12,18,25,8]);assert(r.x>=0&&r.y>=0&&r.x+r.width<=1080&&r.y+r.height<=1920);
console.log('PASS: timeline boundaries, duration conservation, pace, short footage, safe area');
