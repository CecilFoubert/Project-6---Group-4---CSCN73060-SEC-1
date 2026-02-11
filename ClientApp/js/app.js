(function () {
  // Use same origin when served by ASP.NET (e.g. Development)
  const API_BASE = (function () {
    const origin = window.location.origin;
    return origin + '/api/parts';
  })();

  const buildListEl = document.getElementById('buildList');
  const buildTotalEl = document.getElementById('buildTotal');
  const pickerPanel = document.getElementById('pickerPanel');
  const pickerTitle = document.getElementById('pickerTitle');
  const pickerClose = document.getElementById('pickerClose');
  const partsListEl = document.getElementById('partsList');
  const partsMessageEl = document.getElementById('partsMessage');
  const partSearch = document.getElementById('partSearch');
  const clearBuildBtn = document.getElementById('clearBuildBtn');

  const PART_TYPES = [
    'cpu',
    'motherboard',
    'memory',
    'storage',
    'gpu',
    'case',
    'powersupply',
    'cpucooler'
  ];

  const LABELS = {
    cpu: 'CPU',
    motherboard: 'Motherboard',
    memory: 'Memory',
    storage: 'Storage',
    gpu: 'Video Card',
    case: 'Case',
    powersupply: 'Power Supply',
    cpucooler: 'CPU Cooler'
  };

  let currentPartType = null;
  let allParts = [];

  function getBuild() {
    try {
      return JSON.parse(localStorage.getItem('pcpb_build') || '{}');
    } catch {
      return {};
    }
  }

  function saveBuild(build) {
    localStorage.setItem('pcpb_build', JSON.stringify(build));
  }

  function renderBuildRows() {
    const build = getBuild();
    buildListEl.innerHTML = '';

    PART_TYPES.forEach(function (partType) {
      const entry = build[partType];
      const li = document.createElement('li');
      li.className = 'build-row ' + (entry ? 'build-row-filled' : 'build-row-empty');
      li.dataset.partType = partType;

      const label = document.createElement('span');
      label.className = 'build-label';

      if (entry) {
        const nameSpan = document.createElement('span');
        nameSpan.className = 'part-name';
        nameSpan.textContent = entry.Name || 'Part #' + entry.Id;
        const metaSpan = document.createElement('span');
        metaSpan.className = 'part-meta';
        metaSpan.textContent = (entry.Manufacturer ? entry.Manufacturer + ' · ' : '') + (entry.Price ? '$' + entry.Price : '');
        label.appendChild(nameSpan);
        label.appendChild(metaSpan);
      } else {
        label.textContent = LABELS[partType] || partType;
      }

      const action = document.createElement('span');
      action.className = 'build-action';

      if (entry) {
        const priceSpan = document.createElement('span');
        priceSpan.className = 'build-price';
        priceSpan.textContent = entry.Price ? '$' + entry.Price : '';
        action.appendChild(priceSpan);
        const removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.className = 'btn btn-remove';
        removeBtn.textContent = 'Remove';
        removeBtn.addEventListener('click', function (e) {
          e.stopPropagation();
          removeFromBuild(partType);
        });
        action.appendChild(removeBtn);
      } else {
        const chooseBtn = document.createElement('button');
        chooseBtn.type = 'button';
        chooseBtn.className = 'btn btn-choose';
        chooseBtn.textContent = 'Choose';
        chooseBtn.addEventListener('click', function () {
          openPicker(partType);
        });
        action.appendChild(chooseBtn);
      }

      li.appendChild(label);
      li.appendChild(action);
      buildListEl.appendChild(li);
    });

    updateTotal();
  }

  function updateTotal() {
    const build = getBuild();
    let total = 0;
    Object.keys(build).forEach(function (key) {
      const p = parseFloat(build[key].Price);
      if (!isNaN(p)) total += p;
    });
    buildTotalEl.textContent = '$' + (Math.round(total * 100) / 100).toFixed(2);
  }

  function addToBuild(partType, part) {
    const build = getBuild();
    build[partType] = {
      PartType: partType,
      Id: part.Id,
      Name: part.Name,
      Manufacturer: part.Manufacturer,
      Price: part.Price
    };
    saveBuild(build);
    renderBuildRows();
  }

  function removeFromBuild(partType) {
    const build = getBuild();
    delete build[partType];
    saveBuild(build);
    renderBuildRows();
  }

  function clearBuild() {
    saveBuild({});
    renderBuildRows();
  }

  function openPicker(partType) {
    currentPartType = partType;
    pickerTitle.textContent = 'Select ' + (LABELS[partType] || partType);
    partSearch.value = '';
    loadParts(partType);
  }

  function showPartsMessage(msg) {
    partsMessageEl.textContent = msg;
    partsMessageEl.style.display = 'block';
    partsListEl.innerHTML = '';
  }

  function hidePartsMessage() {
    partsMessageEl.style.display = 'none';
  }

  function loadParts(partType) {
    showPartsMessage('Loading...');
    fetch(API_BASE + '/' + encodeURIComponent(partType))
      .then(function (res) {
        if (!res.ok) throw new Error(res.status + ' ' + res.statusText);
        return res.json();
      })
      .then(function (payload) {
        const data = payload.data || payload;
        allParts = Array.isArray(data) ? data : [];
        hidePartsMessage();
        renderParts(allParts);
      })
      .catch(function (err) {
        showPartsMessage('Could not load parts. Make sure the backend is running (e.g. http://localhost:5000) and CORS is enabled. ' + err.message);
      });
  }

  function renderParts(items) {
    partsListEl.innerHTML = '';
    if (!items || items.length === 0) {
      showPartsMessage('No parts found.');
      return;
    }
    hidePartsMessage();

    const fragment = document.createDocumentFragment();
    items.forEach(function (it) {
      const card = document.createElement('div');
      card.className = 'part-card';

      const thumb = document.createElement('div');
      thumb.className = 'part-thumb';
      if (it.ImageUrl) {
        const img = document.createElement('img');
        img.src = it.ImageUrl;
        img.alt = '';
        thumb.appendChild(img);
      } else {
        thumb.textContent = '◈';
      }

      const info = document.createElement('div');
      info.className = 'part-info';
      const name = document.createElement('p');
      name.className = 'part-name';
      name.textContent = it.Name || 'Part #' + (it.Id ?? '');
      const meta = document.createElement('div');
      meta.className = 'part-meta';
      meta.textContent = (it.Manufacturer ? it.Manufacturer + ' · ' : '') + (it.PartNumber ? it.PartNumber : '');
      info.appendChild(name);
      info.appendChild(meta);

      const price = document.createElement('span');
      price.className = 'part-price';
      price.textContent = it.Price ? '$' + it.Price : '';

      const actions = document.createElement('div');
      actions.className = 'part-actions';
      const addBtn = document.createElement('button');
      addBtn.type = 'button';
      addBtn.className = 'btn btn-add';
      addBtn.textContent = 'Add';
      addBtn.addEventListener('click', function () {
        addToBuild(currentPartType, it);
      });
      actions.appendChild(addBtn);

      card.appendChild(thumb);
      card.appendChild(info);
      card.appendChild(price);
      card.appendChild(actions);
      fragment.appendChild(card);
    });
    partsListEl.appendChild(fragment);
  }

  function filterParts() {
    const q = (partSearch.value || '').trim().toLowerCase();
    if (!q) {
      renderParts(allParts);
      return;
    }
    const filtered = allParts.filter(function (p) {
      const name = (p.Name || '').toLowerCase();
      const manu = (p.Manufacturer || '').toLowerCase();
      const partNum = (p.PartNumber || '').toLowerCase();
      return name.indexOf(q) !== -1 || manu.indexOf(q) !== -1 || partNum.indexOf(q) !== -1;
    });
    if (filtered.length === 0) {
      showPartsMessage('No parts match "' + q + '".');
    } else {
      hidePartsMessage();
      renderParts(filtered);
    }
  }

  clearBuildBtn.addEventListener('click', clearBuild);
  pickerClose.addEventListener('click', function () {
    currentPartType = null;
    pickerTitle.textContent = 'Select a part';
    showPartsMessage('Click "Choose" next to a component in Your Build to pick a part.');
    partsListEl.innerHTML = '';
  });

  partSearch.addEventListener('input', filterParts);
  partSearch.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') filterParts();
  });

  renderBuildRows();
  partsMessageEl.textContent = 'Click "Choose" next to a component in Your Build to pick a part.';
  partsMessageEl.style.display = 'block';
})();
