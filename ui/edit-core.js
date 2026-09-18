(function(root){
 'use strict';
 function plan(sourceDuration,target,prompt){
  if(!Number.isFinite(sourceDuration)||sourceDuration<=0)throw Error('Video duration missing');
  target=Math.max(.2,Math.min(Number(target)||15,sourceDuration,300));
  const p=String(prompt).toLowerCase();let pace='balanced',shot=3;
  if(/action|schnell|rasant|fast|teaser|knackig|energet|montage/.test(p)){pace='action';shot=1.35;}
  if(/ruhig|langsam|calm|slow|showcase/.test(p)){pace='calm';shot=5;}
  if(/cinema|epic|episch|trailer/.test(p)){pace='cinematic';shot=2.6;}
  if(/ungeschnitten|uncut|ohne schnitt/.test(p)){pace='uncut';shot=target;}
  const count=Math.max(1,Math.min(120,Math.ceil(target/shot))),duration=target/count,space=sourceDuration-target;
  const segments=Array.from({length:count},(_,i)=>({start:Math.max(0,Math.min(sourceDuration-duration,i*duration+(count>1?space*i/(count-1):space*.35))),duration}));
  return {segments,duration:target,pace};
 }
 function locate(segments,time){let cursor=0;for(let i=0;i<segments.length;i++){let s=segments[i];if(time<cursor+s.duration||i===segments.length-1)return {index:i,source:s.start+Math.max(0,Math.min(s.duration,time-cursor)),offset:time-cursor};cursor+=s.duration;}return null;}
 function safeRect(w,h,m){let v=m.map(n=>Math.min(40,Math.max(0,Number(n)||0))/100);return {x:w*v[3],y:h*v[0],width:w*(1-v[1]-v[3]),height:h*(1-v[0]-v[2])};}
 const api={plan,locate,safeRect};root.EditCore=api;if(typeof module!=='undefined')module.exports=api;
})(typeof window!=='undefined'?window:globalThis);
