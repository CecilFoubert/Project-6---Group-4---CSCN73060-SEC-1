(function () {
  // API Configuration
  const API_BASE = window.location.origin + '/api/parts';
  
  // DOM Elements
  const buildListEl = document.getElementById('buildList');
  const buildTotalEl = document.getElementById('buildTotal');
  const pickerPanel = document.getElementById('pickerPanel');
  const pickerTitle = document.getElementById('pickerTitle');
  const pickerClose = document.getElementById('pickerClose');
  const partsListEl = document.getElementById('partsList');
  const partsMessageEl = document.getElementById('partsMessage');
  const clearBuildBtn = document.getElementById('clearBuildBtn');
  const saveBuildBtn = document.getElementById('saveBuildBtn');
  
  // View Elements
  const browseView = document.getElementById('browseView');
  const buildsView = document.getElementById('buildsView');
  const guidesView = document.getElementById('guidesView');
  const buildsGrid = document.getElementById('buildsGrid');
  const buildsEmpty = document.getElementById('buildsEmpty');
  const newBuildBtn = document.getElementById('newBuildBtn');
  const newBuildBtn2 = document.getElementById('newBuildBtn2');
  
  // Filter Elements
  const filterBtn = document.getElementById('filterBtn');
  const filterModal = document.getElementById('filterModal');
  const filterModalClose = document.getElementById('filterModalClose');
  const filterModalBody = document.getElementById('filterModalBody');
  const clearFiltersBtn = document.getElementById('clearFiltersBtn');
  const applyFiltersBtn = document.getElementById('applyFiltersBtn');
  const activeFiltersCount = document.getElementById('activeFiltersCount');
  
  // Save Build Modal
  const saveBuildModal = document.getElementById('saveBuildModal');
  const saveBuildModalClose = document.getElementById('saveBuildModalClose');
  const buildNameInput = document.getElementById('buildName');
  const buildDescriptionInput = document.getElementById('buildDescription');
  const cancelSaveBtn = document.getElementById('cancelSaveBtn');
  const confirmSaveBtn = document.getElementById('confirmSaveBtn');
  
  // Pagination Elements
  const paginationEl = document.getElementById('pagination');
  const prevPageBtn = document.getElementById('prevPage');
  const nextPageBtn = document.getElementById('nextPage');
  const pageInfo = document.getElementById('pageInfo');
  
  // Navigation
  const navLinks = document.querySelectorAll('.nav-link');
  
  // Part Details Modal
  const partDetailsModal = document.getElementById('partDetailsModal');
  const partDetailsModalClose = document.getElementById('partDetailsModalClose');
  const closeDetailsBtn = document.getElementById('closeDetailsBtn');
  const addFromDetailsBtn = document.getElementById('addFromDetailsBtn');
  
  let currentDetailsPart = null;

  // Constants
  const PART_TYPES = ['cpu', 'motherboard', 'memory', 'storage', 'gpu', 'case', 'powersupply', 'cpucooler'];
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

  // State
  let currentPartType = null;
  let allParts = [];
  let filteredParts = [];
  let currentPage = 1;
  const ITEMS_PER_PAGE = 10;
  let availableFilters = {};
  let activeFilters = {};
  let currentView = 'browse';
  let currentBuildId = null;

  // ===== Navigation =====
  function switchView(viewName) {
    currentView = viewName;
    
    // Update nav links
    navLinks.forEach(link => {
      if (link.dataset.view === viewName) {
        link.classList.add('active');
      } else {
        link.classList.remove('active');
      }
    });
    
    // Hide all views
    browseView.style.display = 'none';
    buildsView.style.display = 'none';
    guidesView.style.display = 'none';
    
    // Show selected view
    if (viewName === 'builds') {
      buildsView.style.display = 'block';
      renderBuildsView();
    } else if (viewName === 'guides') {
      guidesView.style.display = 'block';
    } else {
      browseView.style.display = 'flex';
    }
  }

  navLinks.forEach(link => {
    link.addEventListener('click', (e) => {
      e.preventDefault();
      const view = link.dataset.view;
      switchView(view);
    });
  });

  // ===== Build Management =====
  function getBuild() {
    try {
      return JSON.parse(localStorage.getItem('pcpb_current_build') || '{}');
    } catch {
      return {};
    }
  }

  function saveBuild(build) {
    localStorage.setItem('pcpb_current_build', JSON.stringify(build));
  }

  function getSavedBuilds() {
    try {
      return JSON.parse(localStorage.getItem('pcpb_saved_builds') || '[]');
    } catch {
      return [];
    }
  }

  function saveBuildsToStorage(builds) {
    localStorage.setItem('pcpb_saved_builds', JSON.stringify(builds));
  }

  function loadBuildById(buildId) {
    const builds = getSavedBuilds();
    const build = builds.find(b => b.id === buildId);
    if (build) {
      currentBuildId = buildId;
      saveBuild(build.parts);
      renderBuildRows();
      switchView('browse');
    }
  }

  // ===== Render Build Rows =====
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
        metaSpan.textContent = (entry.Manufacturer ? entry.Manufacturer + ' · ' : '') + (entry.Price ? entry.Price : '');
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
        priceSpan.textContent = entry.Price || '';
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
      const priceStr = (build[key].Price || '').replace(/[^0-9.]/g, '');
      const p = parseFloat(priceStr);
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
    currentBuildId = null;
    saveBuild({});
    renderBuildRows();
  }

  // ===== Save Build =====
  function openSaveBuildModal() {
    const build = getBuild();
    const partCount = Object.keys(build).length;
    
    if (partCount === 0) {
      alert('Please add at least one part to your build before saving.');
      return;
    }

    buildNameInput.value = '';
    buildDescriptionInput.value = '';
    saveBuildModal.style.display = 'flex';
  }

  function closeSaveBuildModal() {
    saveBuildModal.style.display = 'none';
  }

  function saveCurrentBuild() {
    const name = buildNameInput.value.trim();
    if (!name) {
      alert('Please enter a build name');
      return;
    }

    const build = getBuild();
    const builds = getSavedBuilds();
    
    const newBuild = {
      id: currentBuildId || Date.now().toString(),
      name: name,
      description: buildDescriptionInput.value.trim(),
      parts: build,
      createdAt: currentBuildId ? builds.find(b => b.id === currentBuildId)?.createdAt : new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };

    if (currentBuildId) {
      // Update existing
      const index = builds.findIndex(b => b.id === currentBuildId);
      if (index !== -1) {
        builds[index] = newBuild;
      }
    } else {
      // Add new
      builds.push(newBuild);
    }

    saveBuildsToStorage(builds);
    currentBuildId = newBuild.id;
    closeSaveBuildModal();
    alert('Build saved successfully!');
  }

  // ===== Builds View =====
  function renderBuildsView() {
    const builds = getSavedBuilds();
    
    if (builds.length === 0) {
      buildsGrid.innerHTML = '';
      buildsEmpty.style.display = 'block';
      return;
    }

    buildsEmpty.style.display = 'none';
    buildsGrid.innerHTML = '';

    builds.forEach(build => {
      const card = document.createElement('div');
      card.className = 'build-card';
      
      const partCount = Object.keys(build.parts).length;
      let totalPrice = 0;
      Object.values(build.parts).forEach(part => {
        const priceStr = (part.Price || '').replace(/[^0-9.]/g, '');
        const p = parseFloat(priceStr);
        if (!isNaN(p)) totalPrice += p;
      });

      card.innerHTML = `
        <div class="build-card-header">
          <h3>${build.name}</h3>
          <span class="build-date">${new Date(build.updatedAt).toLocaleDateString()}</span>
        </div>
        <div class="build-card-body">
          ${build.description ? `<p class="build-description">${build.description}</p>` : ''}
          <p class="build-stats">${partCount} components · $${totalPrice.toFixed(2)}</p>
        </div>
        <div class="build-card-actions">
          <button type="button" class="btn btn-primary btn-sm" data-action="load" data-id="${build.id}">Load</button>
          <button type="button" class="btn btn-secondary btn-sm" data-action="delete" data-id="${build.id}">Delete</button>
        </div>
      `;

      buildsGrid.appendChild(card);
    });

    // Add event listeners
    buildsGrid.querySelectorAll('[data-action="load"]').forEach(btn => {
      btn.addEventListener('click', () => loadBuildById(btn.dataset.id));
    });

    buildsGrid.querySelectorAll('[data-action="delete"]').forEach(btn => {
      btn.addEventListener('click', () => deleteBuild(btn.dataset.id));
    });
  }

  function deleteBuild(buildId) {
    if (!confirm('Are you sure you want to delete this build?')) return;
    
    const builds = getSavedBuilds();
    const filtered = builds.filter(b => b.id !== buildId);
    saveBuildsToStorage(filtered);
    
    if (currentBuildId === buildId) {
      currentBuildId = null;
      clearBuild();
    }
    
    renderBuildsView();
  }

  // ===== Part Picker =====
  function openPicker(partType) {
    currentPartType = partType;
    pickerTitle.textContent = 'Select ' + (LABELS[partType] || partType);
    activeFilters = {};
    currentPage = 1;
    updateActiveFiltersDisplay();
    loadFiltersAndParts(partType);
  }

  function showPartsMessage(msg) {
    partsMessageEl.textContent = msg;
    partsMessageEl.style.display = 'block';
    partsListEl.innerHTML = '';
    paginationEl.style.display = 'none';
  }

  function hidePartsMessage() {
    partsMessageEl.style.display = 'none';
  }

  async function loadFiltersAndParts(partType) {
    showPartsMessage('Loading...');
    
    try {
      // Load filters
      const filtersRes = await fetch(`${API_BASE}/${partType}/filters`);
      if (filtersRes.ok) {
        const filtersData = await filtersRes.json();
        availableFilters = filtersData.attributes || {};
      }

      // Load parts with filters
      await searchParts();
    } catch (err) {
      showPartsMessage('Error loading parts: ' + err.message);
    }
  }

  async function searchParts() {
    showPartsMessage('Searching...');
    
    try {
      const params = new URLSearchParams(activeFilters);
      const url = `${API_BASE}/${currentPartType}/search?${params}`;
      
      const res = await fetch(url);
      if (!res.ok) throw new Error(res.status + ' ' + res.statusText);
      
      const payload = await res.json();
      allParts = payload.results || payload.data || payload;
      filteredParts = Array.isArray(allParts) ? allParts : [];
      
      currentPage = 1;
      hidePartsMessage();
      renderPartsPage();
    } catch (err) {
      showPartsMessage('Error searching parts: ' + err.message);
    }
  }

  function calculateAveragePart() {
    if (!filteredParts || filteredParts.length === 0) return null;
    
    // Calculate average price
    let totalPrice = 0;
    let priceCount = 0;
    filteredParts.forEach(part => {
      const priceStr = (part.Price || '').replace(/[^0-9.]/g, '');
      const p = parseFloat(priceStr);
      if (!isNaN(p) && p > 0) {
        totalPrice += p;
        priceCount++;
      }
    });
    const avgPrice = priceCount > 0 ? totalPrice / priceCount : 0;
    
    // Count attribute occurrences
    const attributeCounts = {};
    const excludeFields = ['Id', 'Name', 'ImageUrl', 'ProductUrl', 'Price', 'Manufacturer', 'PartNumber', 'PartType', 'SpecsNumber'];
    
    filteredParts.forEach(part => {
      Object.keys(part).forEach(key => {
        if (!excludeFields.includes(key) && part[key] != null && part[key] !== '') {
          if (!attributeCounts[key]) {
            attributeCounts[key] = {};
          }
          const value = String(part[key]);
          attributeCounts[key][value] = (attributeCounts[key][value] || 0) + 1;
        }
      });
    });
    
    // Find most common value for each attribute
    const averagePart = {
      Name: 'Average Part',
      Price: '$' + avgPrice.toFixed(2),
      Manufacturer: 'Statistical Summary',
      PartNumber: `Based on ${filteredParts.length} parts`
    };
    
    Object.keys(attributeCounts).forEach(attr => {
      const values = attributeCounts[attr];
      let mostCommonValue = '';
      let maxCount = 0;
      
      Object.keys(values).forEach(value => {
        if (values[value] > maxCount) {
          maxCount = values[value];
          mostCommonValue = value;
        }
      });
      
      // Add occurrence percentage
      const percentage = ((maxCount / filteredParts.length) * 100).toFixed(0);
      averagePart[attr] = `${mostCommonValue} (${percentage}%)`;
    });
    
    return averagePart;
  }

  function renderPartsPage() {
    if (!filteredParts || filteredParts.length === 0) {
      showPartsMessage('No parts found.');
      return;
    }

    hidePartsMessage();
    
    const totalPages = Math.ceil(filteredParts.length / ITEMS_PER_PAGE);
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    const end = start + ITEMS_PER_PAGE;
    const pageItems = filteredParts.slice(start, end);

    partsListEl.innerHTML = '';
    const fragment = document.createDocumentFragment();
    
    // Add average part at the top
    const avgPart = calculateAveragePart();
    if (avgPart && currentPage === 1) {
      const avgCard = document.createElement('div');
      avgCard.className = 'part-card average-part-card';
      
      const thumb = document.createElement('div');
      thumb.className = 'part-thumb';
      thumb.innerHTML = '<span style="font-size: 32px;">📊</span>';
      
      const info = document.createElement('div');
      info.className = 'part-info';
      const name = document.createElement('p');
      name.className = 'part-name';
      name.textContent = avgPart.Name;
      name.style.cursor = 'pointer';
      name.addEventListener('click', (e) => {
        e.stopPropagation();
        showPartDetails(avgPart, currentPartType);
      });
      const meta = document.createElement('div');
      meta.className = 'part-meta';
      meta.textContent = avgPart.Manufacturer + ' · ' + avgPart.PartNumber;
      info.appendChild(name);
      info.appendChild(meta);
      
      const price = document.createElement('span');
      price.className = 'part-price';
      price.textContent = avgPart.Price;
      
      const actions = document.createElement('div');
      actions.className = 'part-actions';
      const infoBtn = document.createElement('button');
      infoBtn.type = 'button';
      infoBtn.className = 'btn btn-secondary';
      infoBtn.textContent = 'View Stats';
      infoBtn.addEventListener('click', function () {
        showPartDetails(avgPart, currentPartType);
      });
      actions.appendChild(infoBtn);
      
      avgCard.appendChild(thumb);
      avgCard.appendChild(info);
      avgCard.appendChild(price);
      avgCard.appendChild(actions);
      fragment.appendChild(avgCard);
    }

    pageItems.forEach(function (it) {
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
      name.style.cursor = 'pointer';
      name.addEventListener('click', (e) => {
        e.stopPropagation();
        showPartDetails(it, currentPartType);
      });
      const meta = document.createElement('div');
      meta.className = 'part-meta';
      meta.textContent = (it.Manufacturer ? it.Manufacturer + ' · ' : '') + (it.PartNumber ? it.PartNumber : '');
      info.appendChild(name);
      info.appendChild(meta);

      const price = document.createElement('span');
      price.className = 'part-price';
      price.textContent = it.Price || '';

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

    // Update pagination
    pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
    prevPageBtn.disabled = currentPage === 1;
    nextPageBtn.disabled = currentPage === totalPages;
    paginationEl.style.display = 'flex';
  }

  // ===== Filters =====
  function openFilterModal() {
    if (!currentPartType || !availableFilters || Object.keys(availableFilters).length === 0) {
      alert('Please select a part type first');
      return;
    }

    renderFilterOptions();
    filterModal.style.display = 'flex';
  }

  function closeFilterModal() {
    filterModal.style.display = 'none';
  }

  function renderFilterOptions() {
    filterModalBody.innerHTML = '';
    
    // Add price range filter (always visible)
    const priceGroup = document.createElement('div');
    priceGroup.className = 'filter-group';
    priceGroup.innerHTML = `
      <label class="filter-label">Price Range</label>
      <div class="filter-inputs">
        <input type="number" id="filter_minPrice" class="form-input" placeholder="Min" value="${activeFilters.minPrice || ''}" />
        <span>-</span>
        <input type="number" id="filter_maxPrice" class="form-input" placeholder="Max" value="${activeFilters.maxPrice || ''}" />
      </div>
    `;
    filterModalBody.appendChild(priceGroup);

    // Add existing filter rows
    Object.keys(activeFilters).forEach(filterKey => {
      if (filterKey !== 'minPrice' && filterKey !== 'maxPrice') {
        addFilterRow(filterKey, activeFilters[filterKey]);
      }
    });

    // Add "+" button to add new filters
    const addFilterBtn = document.createElement('button');
    addFilterBtn.type = 'button';
    addFilterBtn.className = 'btn btn-secondary add-filter-btn';
    addFilterBtn.innerHTML = '+ Add Filter';
    addFilterBtn.addEventListener('click', () => addFilterRow());
    filterModalBody.appendChild(addFilterBtn);
  }

  function addFilterRow(selectedFilterKey = '', selectedValue = '') {
    const filterRow = document.createElement('div');
    filterRow.className = 'filter-row';
    
    // Get available filter keys (exclude already used ones)
    const usedFilterKeys = new Set();
    filterModalBody.querySelectorAll('.filter-type-select').forEach(select => {
      if (select.value) usedFilterKeys.add(select.value);
    });
    
    const availableFilterKeys = Object.keys(availableFilters).filter(key => 
      availableFilters[key].DistinctValues.length > 0 && 
      (key === selectedFilterKey || !usedFilterKeys.has(key))
    );

    // Filter type dropdown
    const typeSelect = document.createElement('select');
    typeSelect.className = 'form-input filter-type-select';
    
    const defaultOption = document.createElement('option');
    defaultOption.value = '';
    defaultOption.textContent = 'Select filter...';
    typeSelect.appendChild(defaultOption);
    
    availableFilterKeys.forEach(key => {
      const option = document.createElement('option');
      option.value = key;
      option.textContent = formatAttributeName(key);
      if (key === selectedFilterKey) {
        option.selected = true;
      }
      typeSelect.appendChild(option);
    });

    // Filter value dropdown
    const valueSelect = document.createElement('select');
    valueSelect.className = 'form-input filter-value-select';
    valueSelect.disabled = !selectedFilterKey;
    
    if (selectedFilterKey) {
      populateValueDropdown(valueSelect, selectedFilterKey, selectedValue);
    } else {
      const placeholder = document.createElement('option');
      placeholder.value = '';
      placeholder.textContent = 'Select value...';
      valueSelect.appendChild(placeholder);
    }

    // Remove button
    const removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'btn btn-remove filter-remove-btn';
    removeBtn.innerHTML = '×';
    removeBtn.addEventListener('click', () => filterRow.remove());

    // Update value dropdown when type changes
    typeSelect.addEventListener('change', () => {
      const filterKey = typeSelect.value;
      valueSelect.innerHTML = '';
      
      if (filterKey) {
        valueSelect.disabled = false;
        populateValueDropdown(valueSelect, filterKey);
      } else {
        valueSelect.disabled = true;
        const placeholder = document.createElement('option');
        placeholder.value = '';
        placeholder.textContent = 'Select value...';
        valueSelect.appendChild(placeholder);
      }
    });

    filterRow.appendChild(typeSelect);
    filterRow.appendChild(valueSelect);
    filterRow.appendChild(removeBtn);
    
    // Insert before the "Add Filter" button
    const addBtn = filterModalBody.querySelector('.add-filter-btn');
    filterModalBody.insertBefore(filterRow, addBtn);
  }

  function populateValueDropdown(selectElement, filterKey, selectedValue = '') {
    selectElement.innerHTML = '';
    
    const filterData = availableFilters[filterKey];
    if (!filterData) return;
    
    filterData.DistinctValues.forEach(value => {
      const option = document.createElement('option');
      option.value = value;
      option.textContent = value;
      if (value === selectedValue) {
        option.selected = true;
      }
      selectElement.appendChild(option);
    });
  }

  function applyFilters() {
    activeFilters = {};
    
    // Get price filters
    const minPrice = document.getElementById('filter_minPrice')?.value;
    const maxPrice = document.getElementById('filter_maxPrice')?.value;
    if (minPrice) activeFilters.minPrice = minPrice;
    if (maxPrice) activeFilters.maxPrice = maxPrice;
    
    // Get other filters from filter rows
    filterModalBody.querySelectorAll('.filter-row').forEach(row => {
      const typeSelect = row.querySelector('.filter-type-select');
      const valueSelect = row.querySelector('.filter-value-select');
      
      if (typeSelect?.value && valueSelect?.value) {
        activeFilters[typeSelect.value] = valueSelect.value;
      }
    });
    
    updateActiveFiltersDisplay();
    closeFilterModal();
    searchParts();
  }

  function clearFilters() {
    activeFilters = {};
    updateActiveFiltersDisplay();
    if (filterModal.style.display === 'flex') {
      renderFilterOptions();
    } else {
      searchParts();
    }
  }

  function updateActiveFiltersDisplay() {
    const count = Object.keys(activeFilters).length;
    if (count > 0) {
      activeFiltersCount.textContent = `${count} active`;
      activeFiltersCount.style.display = 'inline';
    } else {
      activeFiltersCount.style.display = 'none';
    }
  }

  function formatAttributeName(name) {
    return name.replace(/([A-Z])/g, ' $1').trim();
  }

  // ===== Part Details Modal =====
  function showPartDetails(part, partType) {
    currentDetailsPart = { part, partType };
    
    // Set title
    document.getElementById('partDetailsTitle').textContent = part.Name || 'Part Details';
    
    // Set image
    const imgEl = document.getElementById('partDetailsImage');
    if (part.ImageUrl) {
      imgEl.src = part.ImageUrl;
      imgEl.style.display = 'block';
    } else {
      imgEl.style.display = 'none';
    }
    
    // Set price
    const priceEl = document.getElementById('partDetailsPrice');
    priceEl.textContent = part.Price || 'Price not available';
    
    // Set meta info
    const metaEl = document.getElementById('partDetailsMeta');
    metaEl.innerHTML = '';
    if (part.Manufacturer || part.PartNumber) {
      const metaText = [part.Manufacturer, part.PartNumber].filter(Boolean).join(' · ');
      const p = document.createElement('p');
      p.textContent = metaText;
      metaEl.appendChild(p);
    }
    
    // Set specs
    const specsEl = document.getElementById('partDetailsSpecs');
    specsEl.innerHTML = '';
    
    // Exclude common fields
    const excludeFields = ['Id', 'Name', 'ImageUrl', 'ProductUrl', 'Price', 'Manufacturer', 'PartNumber', 'PartType', 'SpecsNumber'];
    
    // Group specs by category
    const specs = [];
    Object.keys(part).forEach(key => {
      if (!excludeFields.includes(key) && part[key] != null && part[key] !== '') {
        specs.push({ key, value: part[key] });
      }
    });
    
    if (specs.length > 0) {
      const table = document.createElement('table');
      table.className = 'specs-table';
      
      specs.forEach(spec => {
        const row = document.createElement('tr');
        
        const keyCell = document.createElement('td');
        keyCell.className = 'spec-key';
        keyCell.textContent = formatAttributeName(spec.key);
        
        const valueCell = document.createElement('td');
        valueCell.className = 'spec-value';
        valueCell.textContent = spec.value;
        
        row.appendChild(keyCell);
        row.appendChild(valueCell);
        table.appendChild(row);
      });
      
      specsEl.appendChild(table);
    } else {
      specsEl.innerHTML = '<p class="no-specs">No additional specifications available.</p>';
    }
    
    partDetailsModal.style.display = 'flex';
  }

  function closePartDetailsModal() {
    partDetailsModal.style.display = 'none';
    currentDetailsPart = null;
  }

  function addPartFromDetails() {
    if (currentDetailsPart) {
      addToBuild(currentDetailsPart.partType, currentDetailsPart.part);
      closePartDetailsModal();
    }
  }

  // ===== Event Listeners =====
  clearBuildBtn.addEventListener('click', clearBuild);
  saveBuildBtn.addEventListener('click', openSaveBuildModal);
  newBuildBtn.addEventListener('click', () => switchView('browse'));
  newBuildBtn2.addEventListener('click', () => switchView('browse'));
  
  pickerClose.addEventListener('click', function () {
    currentPartType = null;
    pickerTitle.textContent = 'Select a part';
    showPartsMessage('Click "Choose" next to a component in Your Build to pick a part.');
    partsListEl.innerHTML = '';
    paginationEl.style.display = 'none';
  });

  // Filter modal
  filterBtn.addEventListener('click', openFilterModal);
  filterModalClose.addEventListener('click', closeFilterModal);
  clearFiltersBtn.addEventListener('click', clearFilters);
  applyFiltersBtn.addEventListener('click', applyFilters);
  
  // Save build modal
  saveBuildModalClose.addEventListener('click', closeSaveBuildModal);
  cancelSaveBtn.addEventListener('click', closeSaveBuildModal);
  confirmSaveBtn.addEventListener('click', saveCurrentBuild);
  
  // Pagination
  prevPageBtn.addEventListener('click', () => {
    if (currentPage > 1) {
      currentPage--;
      renderPartsPage();
    }
  });
  
  nextPageBtn.addEventListener('click', () => {
    const totalPages = Math.ceil(filteredParts.length / ITEMS_PER_PAGE);
    if (currentPage < totalPages) {
      currentPage++;
      renderPartsPage();
    }
  });

  // Close modals on outside click
  filterModal.addEventListener('click', (e) => {
    if (e.target === filterModal) closeFilterModal();
  });
  
  saveBuildModal.addEventListener('click', (e) => {
    if (e.target === saveBuildModal) closeSaveBuildModal();
  });
  
  partDetailsModal.addEventListener('click', (e) => {
    if (e.target === partDetailsModal) closePartDetailsModal();
  });
  
  // Part details modal
  partDetailsModalClose.addEventListener('click', closePartDetailsModal);
  closeDetailsBtn.addEventListener('click', closePartDetailsModal);
  addFromDetailsBtn.addEventListener('click', addPartFromDetails);

  // ===== Initialize =====
  renderBuildRows();
  partsMessageEl.textContent = 'Click "Choose" next to a component in Your Build to pick a part.';
  partsMessageEl.style.display = 'block';
})();
