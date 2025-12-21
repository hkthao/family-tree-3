import mapboxgl from 'mapbox-gl';
import 'mapbox-gl/dist/mapbox-gl.css';

// =====================================================================================
// IMapboxMapAdapter
// =====================================================================================
export interface IMapboxMapAdapter {
  createMap(options: {
    container: HTMLElement;
    style?: string;
    center?: [number, number];
    zoom?: number;
    accessToken: string;
  }): mapboxgl.Map;
  removeMap(map: mapboxgl.Map): void;
  onMapLoad(map: mapboxgl.Map, callback: () => void): void;
  isMapStyleLoaded(map: mapboxgl.Map): boolean;
}

export const defaultMapboxMapAdapter: IMapboxMapAdapter = {
  createMap: (options) => {
    mapboxgl.accessToken = options.accessToken;
    return new mapboxgl.Map({
      container: options.container,
      style: options.style || 'mapbox://styles/mapbox/streets-v11',
      center: options.center || [106.6297, 10.8231],
      zoom: options.zoom || 10,
    });
  },
  removeMap: (map) => {
    map.remove();
  },
  onMapLoad: (map, callback) => {
    map.on('load', callback);
  },
  isMapStyleLoaded: (map) => {
    return map.isStyleLoaded();
  },
};

// =====================================================================================
// IMapboxMarkerAdapter
// =====================================================================================
export interface IMapboxMarkerAdapter {
  createMarker(options?: { color?: string; anchor?: 'center' | 'top' | 'bottom' | 'left' | 'right' | 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right' }): mapboxgl.Marker;
  setMarkerLngLat(marker: mapboxgl.Marker, lngLat: [number, number]): mapboxgl.Marker;
  addMarkerToMap(marker: mapboxgl.Marker, map: mapboxgl.Map): mapboxgl.Marker;
  removeMarker(marker: mapboxgl.Marker): void;
  createPopup(): mapboxgl.Popup;
  setPopupHtml(popup: mapboxgl.Popup, html: string): mapboxgl.Popup;
  setMarkerPopup(marker: mapboxgl.Marker, popup: mapboxgl.Popup): mapboxgl.Marker;
}

export const defaultMapboxMarkerAdapter: IMapboxMarkerAdapter = {
  createMarker: (options) => new mapboxgl.Marker(options),
  setMarkerLngLat: (marker, lngLat) => marker.setLngLat(lngLat),
  addMarkerToMap: (marker, map) => marker.addTo(map),
  removeMarker: (marker) => marker.remove(),
  createPopup: () => new mapboxgl.Popup(),
  setPopupHtml: (popup, html) => popup.setHTML(html),
  setMarkerPopup: (marker, popup) => marker.setPopup(popup),
};

// =====================================================================================
// IMapboxBoundsAdapter
// =====================================================================================
export interface IMapboxBoundsAdapter {
  createBounds(): mapboxgl.LngLatBounds;
  extendBounds(bounds: mapboxgl.LngLatBounds, lngLat: [number, number]): mapboxgl.LngLatBounds;
  fitMapBounds(map: mapboxgl.Map, bounds: mapboxgl.LngLatBounds, options?: { padding?: number; maxZoom?: number; duration?: number }): void;
}

export const defaultMapboxBoundsAdapter: IMapboxBoundsAdapter = {
  createBounds: () => new mapboxgl.LngLatBounds(),
  extendBounds: (bounds, lngLat) => bounds.extend(lngLat),
  fitMapBounds: (map, bounds, options) => {
    if (!bounds.isEmpty()) {
      map.fitBounds(bounds, { padding: 50, duration: 0, ...options });
    }
  },
};
