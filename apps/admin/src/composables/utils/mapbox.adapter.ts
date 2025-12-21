import mapboxgl from 'mapbox-gl';
import 'mapbox-gl/dist/mapbox-gl.css';
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
  }): any;
  removeMap(map: any): void;
  onMapLoad(map: any, callback: () => void): void;
  isMapStyleLoaded(map: any): boolean;
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
  createMarker(options?: { color?: string; anchor?: 'center' | 'top' | 'bottom' | 'left' | 'right' | 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right' }): any;
  setMarkerLngLat(marker: any, lngLat: [number, number]): any;
  addMarkerToMap(marker: any, map: any): any;
  removeMarker(marker: any): void;
  createPopup(): any;
  setPopupHtml(popup: any, html: string): any;
  setMarkerPopup(marker: any, popup: any): any;
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
  createBounds(): any;
  extendBounds(bounds: any, lngLat: [number, number]): any;
  fitMapBounds(map: any, bounds: any, options?: { padding?: number; maxZoom?: number; duration?: number }): void;
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
