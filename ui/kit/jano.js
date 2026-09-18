/* Shared look transaction. No product state, network, or dependencies. */
(function(global){
  'use strict';
  const valid=s=>s&&/^#[0-9a-f]{6}$/i.test(s.accent)&&['compact','comfortable'].includes(s.density);
  function ink(hex){
    const c=[1,3,5].map(i=>parseInt(hex.slice(i,i+2),16)/255).map(v=>v<=.04045?v/12.92:Math.pow((v+.055)/1.055,2.4));
    return (.2126*c[0]+.7152*c[1]+.0722*c[2])>.179?'#000000':'#ffffff';
  }
  function look(root,key){
    let saved={accent:getComputedStyle(root).getPropertyValue('--j-accent').trim(),density:'compact'};
    try{const s=JSON.parse(localStorage.getItem(key));if(valid(s))saved={accent:s.accent,density:s.density}}catch(_){}
    function apply(s){if(!valid(s))return false;root.style.setProperty('--j-accent',s.accent);root.style.setProperty('--j-accent-ink',ink(s.accent));root.dataset.density=s.density;return true}
    apply(saved);
    return {current:()=>({...saved}),preview:apply,cancel:()=>apply(saved),save(s){
      if(!valid(s))return {ok:false,message:'Bitte einen HEX-Farbwert mit sechs Stellen eingeben.'};
      try{localStorage.setItem(key,JSON.stringify(s))}catch(_){return {ok:false,message:'Speichern ist hier nicht verfügbar. Die Vorschau bleibt ungespeichert.'}}
      saved={accent:s.accent,density:s.density};apply(saved);return {ok:true};
    }};
  }
  global.JanoKit=Object.freeze({look,ink});
})(window);
