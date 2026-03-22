// Shared JavaScript for Build Part Search functionality

let currentPartType = '';
let availableFilters = {};
let partSearchModal;

async function openPartSearch(partType) {
    currentPartType = partType;
    document.getElementById('partSearchModalLabel').textContent = 'Search ' + partType.toUpperCase();
    
    // Load filters for this part type
    await loadFilters(partType);
    
    // Show modal
    partSearchModal.show();
    
    // Perform initial search
    searchParts();
}

async function loadFilters(partType) {
    try {
        const response = await fetch(`/api/parts/${partType}/filters`);
        const data = await response.json();
        availableFilters = data.attributes;
        
        // Build filter controls
        const filterControls = document.getElementById('filterControls');
        filterControls.innerHTML = '';
        
        // Add common filters first
        const commonFilters = ['Manufacturer', 'Name'];
        const otherFilters = Object.keys(availableFilters).filter(f => !commonFilters.includes(f) && f !== 'Price');
        
        // Add Manufacturer filter
        if (availableFilters['Manufacturer']) {
            addFilterControl(filterControls, 'Manufacturer', availableFilters['Manufacturer']);
        }
        
        // Add price range
        addPriceRangeFilter(filterControls);
        
        // Add other important filters (up to 5 more)
        otherFilters.slice(0, 5).forEach(filterName => {
            addFilterControl(filterControls, filterName, availableFilters[filterName]);
        });
        
    } catch (error) {
        console.error('Error loading filters:', error);
    }
}

function addFilterControl(container, filterName, filterData) {
    const col = document.createElement('div');
    col.className = 'col-md-6 mb-3';
    
    const label = document.createElement('label');
    label.className = 'form-label';
    label.textContent = formatAttributeName(filterName);
    
    const select = document.createElement('select');
    select.className = 'form-select';
    select.id = 'filter_' + filterName;
    
    const defaultOption = document.createElement('option');
    defaultOption.value = '';
    defaultOption.textContent = 'Any';
    select.appendChild(defaultOption);
    
    // Add distinct values (limit to first 50 for performance)
    filterData.DistinctValues.slice(0, 50).forEach(value => {
        const option = document.createElement('option');
        option.value = value;
        option.textContent = value;
        select.appendChild(option);
    });
    
    col.appendChild(label);
    col.appendChild(select);
    container.appendChild(col);
}

function addPriceRangeFilter(container) {
    const col = document.createElement('div');
    col.className = 'col-md-6 mb-3';
    
    const label = document.createElement('label');
    label.className = 'form-label';
    label.textContent = 'Price Range';
    
    const inputGroup = document.createElement('div');
    inputGroup.className = 'input-group';
    
    const minInput = document.createElement('input');
    minInput.type = 'number';
    minInput.className = 'form-control';
    minInput.placeholder = 'Min';
    minInput.id = 'filter_minPrice';
    
    const span = document.createElement('span');
    span.className = 'input-group-text';
    span.textContent = '-';
    
    const maxInput = document.createElement('input');
    maxInput.type = 'number';
    maxInput.className = 'form-control';
    maxInput.placeholder = 'Max';
    maxInput.id = 'filter_maxPrice';
    
    inputGroup.appendChild(minInput);
    inputGroup.appendChild(span);
    inputGroup.appendChild(maxInput);
    
    col.appendChild(label);
    col.appendChild(inputGroup);
    container.appendChild(col);
}

async function searchParts() {
    document.getElementById('searchLoading').style.display = 'block';
    document.getElementById('searchResults').innerHTML = '';
    
    try {
        // Build query parameters
        const params = new URLSearchParams();
        
        // Get all filter controls
        document.querySelectorAll('[id^="filter_"]').forEach(control => {
            const filterName = control.id.replace('filter_', '');
            const value = control.value.trim();
            if (value) {
                params.append(filterName, value);
            }
        });
        
        const response = await fetch(`/api/parts/${currentPartType}/search?${params}`);
        const data = await response.json();
        
        displaySearchResults(data.results);
    } catch (error) {
        console.error('Error searching parts:', error);
        document.getElementById('searchResults').innerHTML = '<div class="alert alert-danger">Error searching parts</div>';
    } finally {
        document.getElementById('searchLoading').style.display = 'none';
    }
}

function displaySearchResults(results) {
    const resultsDiv = document.getElementById('searchResults');
    
    if (results.length === 0) {
        resultsDiv.innerHTML = '<div class="alert alert-info">No parts found. Try adjusting your filters.</div>';
        return;
    }
    
    let html = '<div class="table-responsive"><table class="table table-hover table-sm"><thead><tr><th>Name</th><th>Price</th><th>Manufacturer</th><th>Action</th></tr></thead><tbody>';
    
    results.forEach(part => {
        html += `
            <tr>
                <td><small>${part.Name || 'N/A'}</small></td>
                <td><small class="text-success">${part.Price || 'N/A'}</small></td>
                <td><small>${part.Manufacturer || 'N/A'}</small></td>
                <td><button type="button" class="btn btn-sm btn-primary" onclick='selectPart(${JSON.stringify(part)})'>Select</button></td>
            </tr>
        `;
    });
    
    html += '</tbody></table></div>';
    resultsDiv.innerHTML = html;
}

function selectPart(part) {
    // Set the hidden input value
    const partTypeId = currentPartType.toLowerCase() + 'Id';
    document.getElementById(partTypeId).value = part.Id;
    
    // Display selected part
    const selectedDiv = document.getElementById(currentPartType.toLowerCase() + '-selected');
    selectedDiv.innerHTML = `
        <div class="alert alert-success d-flex justify-content-between align-items-center">
            <div>
                <strong>${part.Name}</strong><br>
                <small>${part.Price || 'Price not available'}</small>
            </div>
            <button type="button" class="btn btn-sm btn-danger" onclick="removePart('${currentPartType}')">Remove</button>
        </div>
    `;
    
    // Close modal
    partSearchModal.hide();
}

function removePart(partType) {
    const partTypeId = partType.toLowerCase() + 'Id';
    document.getElementById(partTypeId).value = '';
    document.getElementById(partType.toLowerCase() + '-selected').innerHTML = '';
}

function clearFilters() {
    document.querySelectorAll('[id^="filter_"]').forEach(control => {
        control.value = '';
    });
    searchParts();
}

function formatAttributeName(name) {
    return name.replace(/([A-Z])/g, ' $1').trim();
}
